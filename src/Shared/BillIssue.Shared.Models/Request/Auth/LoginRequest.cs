using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Auth
{
    public class LoginRequest: BaseRequest
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
