using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Auth
{
    public class RemindPasswordRequest: BaseRequest
    {
        public string Email { get; set; }
    }
}
