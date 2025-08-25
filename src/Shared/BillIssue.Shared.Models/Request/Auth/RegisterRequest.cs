using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Auth
{
    public class RegisterRequest: BaseRequest
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
    }
}
