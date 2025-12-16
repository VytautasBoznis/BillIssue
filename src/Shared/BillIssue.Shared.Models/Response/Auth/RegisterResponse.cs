using BillIssue.Shared.Models.Response.Auth.Dto;
using BillIssue.Shared.Models.Response.Base;

namespace BillIssue.Shared.Models.Response.Auth
{
    public class RegisterResponse: BaseResponse
    {
        public SessionDto Session { get; set; }
    }
}
