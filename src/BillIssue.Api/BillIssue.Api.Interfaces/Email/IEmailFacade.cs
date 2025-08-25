namespace BillIssue.Api.Interfaces.Email
{
    public interface IEmailFacade
    {
        public Task SendReminderEmail(string targetEmail, Guid passwordReminderGuid);
    }
}
