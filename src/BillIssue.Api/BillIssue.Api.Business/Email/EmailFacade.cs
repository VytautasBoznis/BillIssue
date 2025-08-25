using BillIssue.Api.Business.Auth;
using BillIssue.Api.Interfaces.Email;
using BillIssue.Api.Models.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BillIssue.Api.Business.Email
{
    public class EmailFacade : IEmailFacade
    {
        private readonly IConfiguration _configuration;
        private readonly SendGridClient _sendGridClient;
        private readonly ILogger<EmailFacade> _logger;

        public EmailFacade(IConfiguration configuration, ILogger<EmailFacade> logger)
        {
            _configuration = configuration;
            _sendGridClient = new SendGridClient(_configuration[AppSettingKeys.SendgridApiKeyKey]);
            _logger = logger;
        }

        public async Task SendReminderEmail(string targetEmail, Guid passwordReminderGuid)
        {
            try
            {
                //TODO MOVE TO DINAMIC TEMPLATES IN THE FUTURE
                EmailAddress fromEmail = new EmailAddress(_configuration[AppSettingKeys.SendgridFromEmailKey], _configuration[AppSettingKeys.SendgridFromEmailDisplayNameKey]);
                string subject = "Password reminder";
                EmailAddress toEmail = new EmailAddress(targetEmail);
                string contentHtml = $"<h1>Password reminder request received</h1></br></br>Click the link to change your password: {string.Format(_configuration[AppSettingKeys.SendgridPasswordReminderLinkTemplateKey], passwordReminderGuid)} </br></br>";

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
