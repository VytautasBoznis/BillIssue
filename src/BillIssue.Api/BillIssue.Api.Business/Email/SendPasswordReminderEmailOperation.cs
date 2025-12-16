using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.ConfigurationOptions;
using BillIssue.Shared.Models.Request.Email;
using BillIssue.Shared.Models.Response.Email;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BillIssue.Api.Business.Email
{
    public class SendPasswordReminderEmailOperation : BaseOperation<SendPasswordReminderEmailRequest, SendPasswordReminderEmailResponse>
    {
        public const string Subject = "Password reminder";
        public const string ContentHtmlTemplate = "<h1>Password reminder request received</h1></br></br>Click the link to change your password: {0} </br></br>";

        public readonly SendgridOptions _sendgridOptions;
        public readonly SendGridClient _sendGridClient;

        public SendPasswordReminderEmailOperation(
            ILogger<SendPasswordReminderEmailOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<SendPasswordReminderEmailRequest> validator,
            IOptions<SendgridOptions> sendgridOptions) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _sendgridOptions = sendgridOptions?.Value ?? throw new ArgumentNullException(nameof(sendgridOptions));
            _sendGridClient = new SendGridClient(_sendgridOptions.ApiKey);
        }

        protected override async Task<SendPasswordReminderEmailResponse> Execute(SendPasswordReminderEmailRequest request, IUnitOfWork unitOfWork)
        {
            try
            {
                //TODO MOVE TO DINAMIC TEMPLATES IN THE FUTURE
                EmailAddress fromEmail = new EmailAddress(_sendgridOptions.FromEmail, _sendgridOptions.FromEmailDisplayName);
                EmailAddress toEmail = new EmailAddress(request.Email);

                SendGridMessage sendgridEmail = MailHelper.CreateSingleEmail(fromEmail, toEmail, Subject, string.Empty, string.Format(ContentHtmlTemplate, string.Format(_sendgridOptions.PasswordReminderLinkTemplate, request.PasswordReminderGuid)));
                await _sendGridClient.SendEmailAsync(sendgridEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error for target email {request.Email} while sending password reminder email: {ex.Message} \n\n Stacktrace: {ex.StackTrace}");
            }

            return new SendPasswordReminderEmailResponse();
        }
    }
}
