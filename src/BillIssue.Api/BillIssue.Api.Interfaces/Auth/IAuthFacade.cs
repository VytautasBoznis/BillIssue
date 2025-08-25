using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Request.Auth;
using BillIssue.Shared.Models.Response.Auth.Dto;

namespace BillIssue.Api.Interfaces.Auth
{
    public interface IAuthFacade
    {
        public Task<SessionDto> Register(RegisterRequest registerRequest);
        public Task<SessionDto> Login(LoginRequest loginRequest);
        public Task RemindPassword(RemindPasswordRequest request);
        public Task RemindPasswordConfirm(RemindPasswordConfirmRequest request);
        public Task ValidateEmail(SessionModel sessionModel, ValidateEmailRequest request);
        public Task ValidateEmailConfirm(ValidateEmailConfirmRequest request);
    }
}
