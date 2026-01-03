using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Operations.Notifications;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Auth;
using BillIssue.Api.Models.ConfigurationOptions;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Authentication;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Request.Auth;
using BillIssue.Shared.Models.Request.Notifications;
using BillIssue.Shared.Models.Response.Auth;
using BillIssue.Shared.Models.Response.Auth.Dto;
using BillIssue.Shared.Models.Response.Notifications;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BillIssue.Api.Business.Operations.Auth
{
    public class LoginOperation : BaseOperation<LoginRequest, LoginResponse>
    {
        private readonly IAuthRepository _authRepository;
        private readonly JwtOptions _jwtOptions;
        
        public LoginOperation(
            ILogger<LoginOperation> logger,
            IValidator<LoginRequest> validator,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IAuthRepository authRepository,
            IOptions<JwtOptions> jwtOptions) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            _authRepository = authRepository;
        }

        protected override async Task<LoginResponse> Execute(LoginRequest request, IUnitOfWork unitOfWork)
        {
            SessionModel? sessionModel = await _authRepository.GetSessionModelByEmail(request.Email, unitOfWork);

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

            GetNotificationsResponse notificationsResponse = await _operationFactory
                                                                        .Get<GetNotificationsOperation>()
                                                                        .Run(new GetNotificationsRequest
                                                                        {
                                                                            SessionUserData = new SessionUserData
                                                                            {
                                                                                Id = sessionModel.Id,
                                                                                Role = sessionModel.Role,
                                                                                Email = request.Email,
                                                                                FirstName = sessionModel.FirstName,
                                                                                LastName = sessionModel.LastName,
                                                                            } 
                                                                        }, unitOfWork);

            return new LoginResponse
            {
                Session = new SessionDto
                {
                    JwtToken = jwtToken,
                    UserId = sessionModel.Id,
                    Email = request.Email,
                    FirstName = sessionModel.FirstName,
                    LastName = sessionModel.LastName,
                    Notifications = notificationsResponse.NotificationDtos,
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
                new(JwtTokenClaimNames.UserId, sessionModel.Id.ToString()),
                new(JwtTokenClaimNames.Email, sessionModel.Email ?? string.Empty),
                new(JwtTokenClaimNames.Role, ((int)sessionModel.Role).ToString()),
                new(JwtTokenClaimNames.FirstName, sessionModel.FirstName ?? string.Empty),
                new(JwtTokenClaimNames.LastName, sessionModel.LastName ?? string.Empty),
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
