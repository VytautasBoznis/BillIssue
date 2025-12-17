using BillIssue.Shared.Models.Request.Notifications;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Notifications
{
    public class DoNotificationDecisionRequestValidator: BaseValidator<DoNotificationDecisionRequest>
    {
        public DoNotificationDecisionRequestValidator()
        {
            RuleFor(x => x.Notification)
                .NotNull().WithMessage("Notification is required");
        }
    }
}
