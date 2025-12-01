namespace BillIssue.Api.Models.ConfigurationOptions
{
    public class SendgridOptions
    {
        public string ApiKey { get; set; }
        public string FromEmail { get; set; }
        public string FromEmailDisplayName { get; set; }
        public string PasswordReminderLinkTemplate { get; set; }
    }
}
