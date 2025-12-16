using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Request.Auth;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using StackExchange.Redis;
using LoginRequest = BillIssue.Shared.Models.Request.Auth.LoginRequest;
using RegisterRequest = BillIssue.Shared.Models.Request.Auth.RegisterRequest;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Interfaces.Email;
using BillIssue.Shared.Models.Constants;
using BillIssue.Api.Interfaces.Workspace;
using BillIssue.Shared.Models.Response.Auth.Dto;
using BillIssue.Shared.Models.Response.Notifications.Dto;
using BillIssue.Api.Models.ConfigurationOptions;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Interfaces.Notifications;

namespace BillIssue.Api.Business.Auth
{
    public class AuthFacade : IAuthFacade
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IDatabaseAsync _redisDBAsync;
        private readonly NpgsqlConnection _dbConnection;
        private readonly ILogger<AuthFacade> _logger;

        private readonly ISessionFacade _sessionFacade;
        private readonly IEmailFacade _emailFacade;
        private readonly IWorkspaceFacade _WorkspaceFacade;
        private readonly INotificationFacade _alertFacade;
        private readonly JwtOptions _jwtOptions;

        private const int PasswordWorkFactor = 12;

        public AuthFacade(
            IConnectionMultiplexer redisConnection,
            NpgsqlConnection dbConnection,
            ILogger<AuthFacade> logger,
            IEmailFacade emailFacade,
            IWorkspaceFacade WorkspaceFacade,
            ISessionFacade sessionFacade,
            INotificationFacade alertFacade,
            IOptions<JwtOptions> jwtOptions
        )
        {
            _redisConnection = redisConnection;
            _dbConnection = dbConnection;
            _redisDBAsync = _redisConnection.GetDatabase();
            _logger = logger;
            _WorkspaceFacade = WorkspaceFacade;
            _alertFacade = alertFacade;
            _sessionFacade = sessionFacade;
            _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        }

        #region Facade implementation

        public async Task<SessionDto> Login(LoginRequest loginRequest)
        {
            return await LoginToSession(loginRequest.Email, loginRequest.Password);
        }

        public async Task<SessionDto> Register(RegisterRequest registerRequest)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password, PasswordWorkFactor);
            try
            {
                var dictionary = new Dictionary<string, object> { { "@email", registerRequest.Email } };
                SessionModel? sessionModel = _dbConnection.Query<SessionModel>("SELECT email FROM user_users WHERE LOWER(email)=LOWER(@email)", dictionary).FirstOrDefault();

                if (sessionModel != null)
                {
                    throw new RegistrationException("Email already in use", ExceptionCodes.AUTH_REGISTRATION_EMAIL_ALREADY_IN_USE);
                }

                Guid newUserId = Guid.NewGuid();

                NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

                await using NpgsqlCommand insertUser = new NpgsqlCommand("INSERT INTO user_users (id, email, password, first_name, last_name, created_by) VALUES (@id, @email, @pass, @firstName, @lastName, @createdBy)", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@id", newUserId),
                        new("@email", registerRequest.Email),
                        new("@pass", passwordHash),
                        new("@firstName", registerRequest.FirstName),
                        new("@lastName", registerRequest.LastName),
                        new("@createdBy", registerRequest.Email),
                    }
                };

                await insertUser.ExecuteNonQueryAsync();

                await _WorkspaceFacade.CreatePersonalWorkspaceForNewUser(newUserId, registerRequest.FirstName, registerRequest.LastName, transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (ex is RegistrationException)
                {
                    throw;
                }

                _logger.LogError($"A not standart exception happened in registration: {ex.Message} \n\n Stacktrace: {ex.StackTrace}");
                throw new BaseAppException("Error occured during registration!", errorCode: ExceptionCodes.UNEXPECTED_EXCEPTION);
            }

            return await LoginToSession(registerRequest.Email, registerRequest.Password);
        }
         
        public async Task RemindPassword(RemindPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                _logger.LogError($"A change password attempt was made without email input");
                throw new ArgumentNullException(nameof(request.Email));
            }

            var dictionary = new Dictionary<string, object> { { "@email", request.Email } };
            SessionModel? sessionModel = _dbConnection.Query<SessionModel>("SELECT id, email FROM user_users WHERE LOWER(email)=LOWER(@email)", dictionary).FirstOrDefault();

            if (sessionModel == null || sessionModel.Id == Guid.Empty)
            {
                _logger.LogWarning($"A password remind request was issued for an email that does not exist: {request.Email}");
                return;
            }

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            Guid passwordReminderGuid = Guid.NewGuid();

            try
            {
                await using NpgsqlCommand insertPasswordReminder = new NpgsqlCommand("INSERT INTO user_confirmations (id, user_id, confirmation_type, created_by) VALUES (@id, @userId, @confirmationType, @createdBy)", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@id", passwordReminderGuid),
                        new("@userId", sessionModel.Id),
                        new("@confirmationType", (int) ConfirmationTypeEnum.RemindPassword),
                        new("@createdBy", "System password reminder"),
                    }
                };

                await insertPasswordReminder.ExecuteNonQueryAsync();
                await _emailFacade.SendReminderEmail(request.Email, passwordReminderGuid);

                transaction.Commit();
            }
            catch (Exception ex)
            {

                _logger.LogError($"""
                    Failed to create and send password reminder for email: {request.Email} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                transaction.Rollback();
            }
        }

        public async Task RemindPasswordConfirm(RemindPasswordConfirmRequest request)
        {
            if (request.PasswordReminderGuid == Guid.Empty)
            {
                _logger.LogError($"A password remind request was made without the confirmation guid");
                throw new ArgumentNullException(nameof(request.PasswordReminderGuid));
            }

            var dictionary = new Dictionary<string, object> { { "@confirmation_id", request.PasswordReminderGuid } };
            UserConfirmationModel? userConfirmationModel = _dbConnection.Query<UserConfirmationModel>("SELECT id, user_id, confirmation_type FROM user_confirmations WHERE id = @confirmation_id", dictionary).FirstOrDefault();

            if (userConfirmationModel == null)
            {
                _logger.LogWarning($"An attempt to use an old passoword reminder id {request.PasswordReminderGuid} was made, no user confirmation model found, return a success");
                return;
            }

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            try
            {
                await ChangeUserPassword(userConfirmationModel.UserId, request.Password, request.ConfirmPassword, "System", transaction);

                await using NpgsqlCommand deleteUserConfirmation = new NpgsqlCommand("DELETE FROM user_confirmations WHERE id = @userConfirmationId", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@userConfirmationId", request.PasswordReminderGuid)
                    }
                };

                await deleteUserConfirmation.ExecuteNonQueryAsync();

                transaction.Commit();
            }
            catch (Exception ex)
            {

                _logger.LogError($"""
                    Failed to change password for password reminder confirmation: {request.PasswordReminderGuid} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                transaction.Rollback();
            }
        }

        public Task ValidateEmail(SessionModel sessionModel, ValidateEmailRequest request)
        {
            throw new NotImplementedException();
        }

        public Task ValidateEmailConfirm(ValidateEmailConfirmRequest request)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private functions

        private async Task<SessionDto> LoginToSession(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogError($"A login attempt was made without email input");
                throw new ArgumentNullException(nameof(email));
            }

            if (string.IsNullOrEmpty(password))
            {
                _logger.LogError($"A login attempt was made without passwrod input");
                throw new ArgumentNullException(nameof(password));
            }

            var dictionary = new Dictionary<string, object> { { "@email", email } };
            SessionModel? sessionModel = _dbConnection.Query<SessionModel>("SELECT id, password, email, role, is_banned as isBanned, first_name as FirstName, last_name as LastName FROM user_users WHERE email = @email", dictionary).FirstOrDefault();

            if (sessionModel == null || !BCrypt.Net.BCrypt.Verify(password, sessionModel.Password))
            {
                throw new LoginException("Email or password did not match", ExceptionCodes.AUTH_EMAIL_AND_PASSWORD_MISSMATCH);
            }

            if (sessionModel.IsBanned)
            {
                throw new LoginException("There is an issue with your account, please contact support for further details", ExceptionCodes.AUTH_CONTACT_SUPPORT);
            }

            // preserve existing session behavior (stores session in Redis and returns guid)
            Guid authToken = await _sessionFacade.SetSession(sessionModel);

            // generate JWT for the authenticated user
            string jwtToken = GenerateJwtToken(sessionModel);

            List<NotificationDto> userNotifications = _alertFacade.GetWorkspaceNotificationAsNotifications(email);

            return new SessionDto
            {
                AuthToken = authToken,
                JwtToken = jwtToken,
                UserId = sessionModel.Id,
                Email = email,
                FirstName = sessionModel.FirstName,
                LastName = sessionModel.LastName,
                Notifications = userNotifications,
            };
        }

        private string GenerateJwtToken(SessionModel sessionModel)
        {
            if (string.IsNullOrEmpty(_jwtOptions?.SecretKey))
            {
                _logger.LogError("JWT SecretKey is not configured. Cannot generate JWT token.");
                throw new InvalidOperationException("JWT SecretKey is not configured.");
            }

            var claims = new List<Claim>
            {
                new Claim(JwtTokenClaimNames.UserId, sessionModel.Id.ToString()),
                new Claim(JwtTokenClaimNames.Email, sessionModel.Email ?? string.Empty),
                new Claim(JwtTokenClaimNames.Role, ((int)sessionModel.Role).ToString()),
                new Claim(JwtTokenClaimNames.FirstName, sessionModel.FirstName ?? string.Empty),
                new Claim(JwtTokenClaimNames.LastName, sessionModel.LastName ?? string.Empty),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryInMinutes > 0 ? _jwtOptions.ExpiryInMinutes : 60);

            var token = new JwtSecurityToken(
                issuer: string.IsNullOrEmpty(_jwtOptions.Issuer) ? null : _jwtOptions.Issuer,
                audience: string.IsNullOrEmpty(_jwtOptions.Audience) ? null : _jwtOptions.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task ChangeUserPassword(Guid userId, string password, string passwordConfirmation, string modifiedBy, NpgsqlTransaction transaction)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordConfirmation) || password != passwordConfirmation)
            {
                _logger.LogError("Password change failed, because there is an issue with password, password confirmation or they don't match");
                throw new BaseAppException("Failed to update user password", errorCode: ExceptionCodes.AUTH_FAILED_TO_UPDATE_USER);
            }

            if (userId == Guid.Empty)
            {
                _logger.LogError("Password change failed, no user id provided");
                throw new ArgumentNullException(nameof(userId));
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, PasswordWorkFactor);

            await using NpgsqlCommand updateUserPassword = new NpgsqlCommand("UPDATE user_users SET password = @passwordHash, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE id = @userId", _dbConnection, transaction)
            {
                Parameters =
                {
                    new("@userId", userId),
                    new("@passwordHash", passwordHash),
                    new("@modifiedBy", modifiedBy),
                    new("@modifiedOn", DateTime.Now),
                }
            };

            await updateUserPassword.ExecuteNonQueryAsync();
        }

        #endregion
    }
}
