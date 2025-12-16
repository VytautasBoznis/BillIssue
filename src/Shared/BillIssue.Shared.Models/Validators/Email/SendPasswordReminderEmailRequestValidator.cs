using BillIssue.Shared.Models.Request.Email;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Email
{
    public class SendPasswordReminderEmailRequestValidator: AbstractValidator<SendPasswordReminderEmailRequest>
    {
        public SendPasswordReminderEmailRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");
            RuleFor(x => x.PasswordReminderGuid)
                .NotEmpty().WithMessage("Password reminder GUID is required");
        }
    }
}
