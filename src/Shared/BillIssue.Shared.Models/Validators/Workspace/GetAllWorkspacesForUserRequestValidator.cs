using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Workspace
{
    public class GetAllWorkspacesForUserRequestValidator : BaseValidator<GetAllWorkspacesForUserRequest>
    {
        public GetAllWorkspacesForUserRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");
        }
    }
}
