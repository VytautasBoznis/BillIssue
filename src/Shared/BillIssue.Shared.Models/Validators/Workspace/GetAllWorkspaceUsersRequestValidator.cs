using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Workspace
{
    public class GetAllWorkspaceUsersRequestValidator : BaseValidator<GetAllWorkspaceUsersRequest>
    {
        public GetAllWorkspaceUsersRequestValidator()
        {
            RuleFor(x => x.WorkspaceId)
                .NotEmpty().WithMessage("WorkspaceId is required.")
                .Must(id => id != Guid.Empty).WithMessage("WorkspaceId must be a valid GUID.");
        }
    }
}
