using BillIssue.Api.Models.Models.User;
using BillIssue.Shared.Models.Response.User.Dto;

namespace BillIssue.Api.Interfaces.User
{
    public interface IUserFacade
    {
        public Task<SessionUserDto> GetCurrentSessionUserData(string sessionId);
    }
}
