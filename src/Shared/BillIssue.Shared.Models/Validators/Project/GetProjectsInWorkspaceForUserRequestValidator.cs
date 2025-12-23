using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class GetProjectsInWorkspaceForUserRequestValidator : BaseValidator<GetProjectsInWorkspaceForUserRequest>
    {
        public GetProjectsInWorkspaceForUserRequestValidator()
        {
            RuleFor(x => x.WorkspaceId)
                .NotEmpty().WithMessage("WorkspaceId is required");
        }
    }
}
