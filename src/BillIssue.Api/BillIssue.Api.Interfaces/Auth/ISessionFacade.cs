using BillIssue.Api.Models.Models.Auth;

namespace BillIssue.Api.Interfaces.Auth
{
    public interface ISessionFacade
    {
        public Task<Guid> SetSession(SessionModel sessionModel);
        public Task<SessionModel> GetSessionModel(string sessionId);
        public Task<SessionModel> GetSessionModelFromJwt(string jwtToken);
    }
}
