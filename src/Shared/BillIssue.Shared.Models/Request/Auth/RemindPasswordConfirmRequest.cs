using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Auth
{
    public class RemindPasswordConfirmRequest: BaseRequest
    {
        public Guid PasswordReminderGuid { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
