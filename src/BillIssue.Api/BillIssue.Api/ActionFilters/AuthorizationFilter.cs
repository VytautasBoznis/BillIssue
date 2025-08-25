using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BillIssue.Api.ActionFilters
{
    public class AuthorizationFilter: IAsyncActionFilter
    {
        public UserRole MinimumRole { get; set; }
        private readonly TimeSpan expandedSessionByTimespan = TimeSpan.FromMinutes(AuthConstants.SessionTimeInMinutes);

        public AuthorizationFilter(UserRole minimumRole = UserRole.User)
        {
            MinimumRole = minimumRole;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string authToken = context.HttpContext.Request.Headers[AuthConstants.AuthTokenHeaderName];

            if (string.IsNullOrEmpty(authToken))
            {
                throw new UnauthorizedException("Unauthorized", "User unauthorized", System.Net.HttpStatusCode.Unauthorized, "USER_UNAUTHORIZED");
            }

            IDatabaseAsync? redis = context.HttpContext.RequestServices.GetService<IConnectionMultiplexer>()?.GetDatabase();

            if (redis == null)
            {
                throw new BaseAppException("Failed to check authorization");
            }

            string sessionData = await redis.StringGetAsync(authToken);

            if (sessionData == null)
            {
                throw new UnauthorizedException("Unauthorized", "User unauthorized", System.Net.HttpStatusCode.Unauthorized, "USER_UNAUTHORIZED");
            }

            SessionModel sessionModel = JsonConvert.DeserializeObject<SessionModel>(sessionData);

            if (sessionModel == null)
            {
                throw new UnauthorizedException("Unauthorized", "User unauthorized", System.Net.HttpStatusCode.Unauthorized, "USER_UNAUTHORIZED");
            }

            if (sessionModel.Role < MinimumRole)
            {
                throw new UnauthorizedException("Unauthorized", "User unauthorized", System.Net.HttpStatusCode.Unauthorized, "USER_UNAUTHORIZED");
            }

            await redis.StringSetAsync(authToken, sessionData, expandedSessionByTimespan);

            await next();
        }
    }
}
