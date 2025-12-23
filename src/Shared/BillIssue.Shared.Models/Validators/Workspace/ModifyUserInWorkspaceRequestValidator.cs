using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Workspace
{
    public class ModifyUserInWorkspaceRequestValidator : BaseValidator<ModifyUserInWorkspaceRequest>
    {
        public ModifyUserInWorkspaceRequestValidator()
        {
            RuleFor(x => x.WorkspaceId)
                .NotEmpty().WithMessage("WorkspaceId is required.")
                .Must(id => id != Guid.Empty).WithMessage("WorkspaceId must be a valid GUID.");
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");
            RuleFor(x => x.NewUserRole)
                .IsInEnum().WithMessage("Invalid language type specified.");
        }
    }
}
