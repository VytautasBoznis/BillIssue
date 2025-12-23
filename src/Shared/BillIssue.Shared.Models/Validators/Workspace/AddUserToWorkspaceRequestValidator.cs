using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Workspace
{
    public class AddUserToWorkspaceRequestValidator : BaseValidator<AddUserToWorkspaceRequest>
    {
        public AddUserToWorkspaceRequestValidator()
        {
            RuleFor(x => x.WorkspaceId)
                .NotEmpty().WithMessage("WorkspaceId is required.")
                .Must(id => id != Guid.Empty).WithMessage("WorkspaceId must be a valid GUID.");
            RuleFor(x => x.NewUserEmail)
                .NotEmpty().WithMessage("UserEmail is required.")
                .EmailAddress().WithMessage("UserEmail must be a valid email address.");
        }
    }
}
