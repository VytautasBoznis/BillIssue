using BillIssue.Api.Interfaces.Email;
using BillIssue.Api.Models.ConfigurationOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BillIssue.Api.Business.Email
{
    public class EmailFacade : IEmailFacade
    {
        private readonly SendgridOptions _configs;
        private readonly SendGridClient _sendGridClient;
        private readonly ILogger<EmailFacade> _logger;

        public EmailFacade(IOptions<SendgridOptions> configs, ILogger<EmailFacade> logger)
        {
            _configs = configs.Value;
            _sendGridClient = new SendGridClient(_configs.ApiKey);
            _logger = logger;
        }

        public async Task SendReminderEmail(string targetEmail, Guid passwordReminderGuid)
        {
            try
            {
                //TODO MOVE TO DINAMIC TEMPLATES IN THE FUTURE
                EmailAddress fromEmail = new EmailAddress(_configs.FromEmail, _configs.FromEmailDisplayName);
                string subject = "Password reminder";
                EmailAddress toEmail = new EmailAddress(targetEmail);
                string contentHtml = $"<h1>Password reminder request received</h1></br></br>Click the link to change your password: {string.Format(_configs.PasswordReminderLinkTemplate, passwordReminderGuid)} </br></br>";

                SendGridMessage sendgridEmail = MailHelper.CreateSingleEmail(fromEmail, toEmail, subject, string.Empty, contentHtml);
                await _sendGridClient.SendEmailAsync(sendgridEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error for target email {targetEmail} while sending password reminder email: {ex.Message} \n\n Stacktrace: {ex.StackTrace}");
            }
        }
    }
}
