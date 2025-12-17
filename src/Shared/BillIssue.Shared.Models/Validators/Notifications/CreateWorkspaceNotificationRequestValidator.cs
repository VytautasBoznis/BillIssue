using BillIssue.Shared.Models.Request.Notifications;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Notifications
{
    public class CreateWorkspaceNotificationRequestValidator: BaseValidator<CreateWorkspaceNotificationRequest>
    {
        public CreateWorkspaceNotificationRequestValidator()
        {
            RuleFor(x => x.WorkspaceId)
                .NotEmpty().WithMessage("WorkspaceId is required.");
            RuleFor(x => x.TargetUserEmail)
                .NotEmpty().WithMessage("TargetUserEmail is required.")
                .EmailAddress().WithMessage("TargetUserEmail must be a valid email address.");
        }
    }
}
