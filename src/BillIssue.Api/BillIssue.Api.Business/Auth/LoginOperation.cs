using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.ConfigurationOptions;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Request.Auth;
using BillIssue.Shared.Models.Response.Auth;
using BillIssue.Shared.Models.Response.Auth.Dto;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BillIssue.Api.Business.Auth
{
    public class LoginOperation : BaseOperation<LoginRequest, LoginResponse>
    {
        private readonly JwtOptions _jwtOptions;
        private readonly NpgsqlConnection _dbConnection;

        public LoginOperation(
            ILogger<LoginOperation> logger,
            IValidator<LoginRequest> validator,
            NpgsqlConnection dbConnection,
            IOptions<JwtOptions> jwtOptions) : base(logger, validator)
        {

            _dbConnection = dbConnection;
            _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        }

        protected override LoginResponse Execute(LoginRequest request)
        {
            var dictionary = new Dictionary<string, object> { { "@email", request.Email } };
            SessionModel? sessionModel = _dbConnection.Query<SessionModel>("SELECT id, password, email, role, is_banned as isBanned, first_name as FirstName, last_name as LastName FROM user_users WHERE email = @email", dictionary).FirstOrDefault();

            if (sessionModel == null || !BCrypt.Net.BCrypt.Verify(request.Password, sessionModel.Password))
            {
                throw new LoginException("Email or password did not match", ExceptionCodes.AUTH_EMAIL_AND_PASSWORD_MISSMATCH);
            }

            if (sessionModel.IsBanned)
            {
                throw new LoginException("There is an issue with your account, please contact support for further details", ExceptionCodes.AUTH_CONTACT_SUPPORT);
            }

            // generate JWT for the authenticated user
            string jwtToken = GenerateJwtToken(sessionModel);

            //List<NotificationDto> userNotifications = _alertFacade.GetWorkspaceNotificationAsNotifications(email);

            return new LoginResponse
            {
                Session = new SessionDto
                {
                    JwtToken = jwtToken,
                    UserId = sessionModel.Id,
                    Email = request.Email,
                    FirstName = sessionModel.FirstName,
                    LastName = sessionModel.LastName,
                    //Notifications = userNotifications,
                }
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
    }
}
