using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Workspace
{
    public class GetWorkspaceSelectionsForUserRequestValidator : BaseValidator<GetWorkspaceSelectionsForUserRequest>
    {
        public GetWorkspaceSelectionsForUserRequestValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        }
    }
}
