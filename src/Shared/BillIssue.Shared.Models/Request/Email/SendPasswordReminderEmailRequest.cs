using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Email
{
    public class SendPasswordReminderEmailRequest: BaseRequest
    {
        public string Email { get; set; }
        public Guid PasswordReminderGuid { get; set; }
    }
}
