namespace BillIssue.Api.Models.Constants
{
    public static class AuthConstants
    {
        public const string BearerPrefix = "Bearer";
        public const string BearerAuthorizationHeaderName = "Authorization";

        public const string UserRequiredPolicyName = "UserRequired";
        public const string AdminRequiredPolicyName = "AdminRequired";

        public const int SessionTimeInMinutes = 20;
        public const string AuthTokenHeaderName = "AuthToken";
    }
}
