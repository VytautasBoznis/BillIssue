using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Shared.Models.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BillIssue.Api.Controllers.Base
{
    public class BaseController : ControllerBase
    {
        protected readonly ILogger _logger;
        
        public BaseController(ILogger logger)
        {
            _logger = logger;
        }

        protected SessionUserData GetSessionModelFromJwt()
        {
            if (Request.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                throw new SessionExpiredException("Invalid token");
            }

            ClaimsPrincipal user = Request.HttpContext.User!;

            string sub = user.FindFirst(JwtTokenClaimNames.UserId)?.Value
                               ?? user.FindFirst(ClaimTypes.Sid)?.Value;

            string email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                               ?? user.FindFirst(ClaimTypes.Email)?.Value;

            string roleClaim = user.FindFirst(JwtTokenClaimNames.Role)?.Value
                               ?? user.FindFirst(ClaimTypes.Role)?.Value;

            string firstName = user.FindFirst(JwtTokenClaimNames.FirstName)?.Value;
            string lastName = user.FindFirst(JwtTokenClaimNames.LastName)?.Value;

            if (!Guid.TryParse(sub, out Guid userId))
            {
                _logger.LogWarning($"Invalid or missing user id in JWT for user. Melformed token detected");
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

            return new SessionUserData
            {
                Id = userId,
                Email = email ?? string.Empty,
                Role = role,
                FirstName = firstName ?? string.Empty,
                LastName = lastName ?? string.Empty
            };
        }
    }
}
