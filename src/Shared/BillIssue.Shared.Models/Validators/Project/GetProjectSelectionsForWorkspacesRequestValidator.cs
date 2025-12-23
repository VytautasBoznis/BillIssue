using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class GetProjectSelectionsForWorkspacesRequestValidator: BaseValidator<GetProjectSelectionsForWorkspacesRequest>
    {
        public GetProjectSelectionsForWorkspacesRequestValidator()
        {
            RuleFor(x => x.WorkspaceIds).NotEmpty().WithMessage("WorkspaceIds are required.");
        }
    }
}
