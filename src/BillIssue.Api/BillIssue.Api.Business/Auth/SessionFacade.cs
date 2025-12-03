using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.ConfigurationOptions;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BillIssue.Api.Models.Enums.Auth;

namespace BillIssue.Api.Business.Auth
{
    public class SessionFacade : ISessionFacade
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IDatabaseAsync _redisDBAsync;
        private readonly ILogger<SessionFacade> _logger;
        private readonly JwtOptions _jwtOptions;

        private readonly TimeSpan SessionDuration = TimeSpan.FromMinutes(AuthConstants.SessionTimeInMinutes);

        public SessionFacade(
            IConnectionMultiplexer redisConnection,
            ILogger<SessionFacade> logger,
            IOptions<JwtOptions> jwtOptions)
        {
            _redisConnection = redisConnection;
            _redisDBAsync = _redisConnection.GetDatabase();
            _logger = logger;
            _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        }

        public async Task<Guid> SetSession(SessionModel sessionModel)
        {
            Guid sessionGuid = Guid.NewGuid();
            await _redisDBAsync.StringSetAsync(sessionGuid.ToString(), JsonConvert.SerializeObject(sessionModel), SessionDuration);
            return sessionGuid;
        }

        public async Task<SessionModel> GetSessionModel(string sessionId)
        {
            var sessionModelString = await _redisDBAsync.StringGetAsync(sessionId);
            SessionModel sessionModel;

            if (sessionModelString == RedisValue.Null)
            {
                _logger.LogError($"Session not found for session id: {sessionId}");
                throw new SessionExpiredException("Session not found");
            }

            try
            {
                sessionModel = JsonConvert.DeserializeObject<SessionModel>(sessionModelString);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to desiarialized session object from cache: {ex.Message}");
                throw new SessionExpiredException("Session not found");
            }

            return sessionModel;
        }

        public Task<SessionModel> GetSessionModelFromJwt(string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                _logger.LogError("Empty JWT token provided to GetSessionModelFromJwt");
                throw new SessionExpiredException("Invalid token");
            }

            string token = jwtToken.StartsWith(AuthConstants.BearerPrefix, StringComparison.OrdinalIgnoreCase)
                ? jwtToken[AuthConstants.BearerPrefix.Length..].Trim()
                : jwtToken.Trim();

            if (string.IsNullOrEmpty(_jwtOptions?.SecretKey))
            {
                _logger.LogError("JWT SecretKey is not configured in JwtOptions");
                throw new InvalidOperationException("JWT not configured");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
                ValidateIssuer = !string.IsNullOrEmpty(_jwtOptions.Issuer),
                ValidIssuer = _jwtOptions.Issuer,
                ValidateAudience = !string.IsNullOrEmpty(_jwtOptions.Audience),
                ValidAudience = _jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                JwtSecurityToken jwt = validatedToken as JwtSecurityToken;

                if (jwt == null)
                {
                    _logger.LogError("Validated token is not a JWT token");
                    throw new SessionExpiredException("Invalid token");
                }

                string sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                string email = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
                string roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtTokenClaimNames.Role)?.Value;
                string firstName = jwt.Claims.FirstOrDefault(c => c.Type == JwtTokenClaimNames.FirstName)?.Value;
                string lastName = jwt.Claims.FirstOrDefault(c => c.Type == JwtTokenClaimNames.LastName)?.Value;

                if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out Guid userId))
                {
                    _logger.LogError("JWT does not contain a valid subject (user id)");
                    throw new SessionExpiredException("Invalid token");
                }

                UserRole role = UserRole.User;

                if (!string.IsNullOrEmpty(roleClaim) && int.TryParse(roleClaim, out int roleInt))
                {
                    role = (UserRole)roleInt;
                }
                else
                {
                    _logger.LogWarning($"Invalid or missing role claim in JWT for user id: {userId}. Melformed token detected");
                    throw new SessionExpiredException("Invalid token");
                }

                var sessionModel = new SessionModel
                {
                    Id = userId,
                    Email = email ?? string.Empty,
                    Role = role,
                    FirstName = firstName ?? string.Empty,
                    LastName = lastName ?? string.Empty,
                    Password = string.Empty,
                    IsBanned = false
                };

                return Task.FromResult(sessionModel);
            }
            catch (SecurityTokenException stex)
            {
                _logger.LogWarning($"JWT validation failed: {stex.Message}");
                throw new SessionExpiredException("Invalid or expired token");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while parsing JWT: {ex.Message}");
                throw new SessionExpiredException("Invalid or expired token");
            }
        }
    }
}
