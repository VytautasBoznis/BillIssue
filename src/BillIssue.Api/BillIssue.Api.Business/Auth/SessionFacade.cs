using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BillIssue.Api.Business.Auth
{
    public class SessionFacade : ISessionFacade
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IDatabaseAsync _redisDBAsync;
        private readonly ILogger<AuthFacade> _logger;

        private readonly TimeSpan SessionDuration = TimeSpan.FromMinutes(AuthConstants.SessionTimeInMinutes);

        public SessionFacade(
            IConnectionMultiplexer redisConnection,
            ILogger<AuthFacade> logger)
        {
            _redisConnection = redisConnection;
            _redisDBAsync = _redisConnection.GetDatabase();
            _logger = logger;
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
                _logger.LogError($"Failed to desiarialized session object from cache");
                throw new SessionExpiredException("Session not found");
            }

            return sessionModel;
        }
    }
}
