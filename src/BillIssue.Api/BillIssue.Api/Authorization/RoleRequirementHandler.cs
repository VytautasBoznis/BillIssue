using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Constants;

namespace BillIssue.Api.Authorization
{
    public class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return Task.CompletedTask;
            }

            string? roleClaim = context.User.FindFirst(JwtTokenClaimNames.Role)?.Value
                               ?? context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(roleClaim))
            {
                return Task.CompletedTask;
            }

            if (!int.TryParse(roleClaim, out int roleInt))
            {
                return Task.CompletedTask;
            }

            if (!Enum.IsDefined(typeof(UserRole), roleInt))
            {
                return Task.CompletedTask;
            }

            var userRole = (UserRole)roleInt;

            if (userRole >= requirement.MinimumRole)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
