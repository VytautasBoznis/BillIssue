using Microsoft.AspNetCore.Authorization;
using BillIssue.Api.Models.Enums.Auth;

namespace BillIssue.Api.Authorization
{
    public class RoleRequirement : IAuthorizationRequirement
    {
        public UserRole MinimumRole { get; }

        public RoleRequirement(UserRole minimumRole)
        {
            MinimumRole = minimumRole;
        }
    }
}
