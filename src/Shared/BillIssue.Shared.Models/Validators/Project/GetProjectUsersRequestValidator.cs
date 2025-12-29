using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class GetProjectUsersRequestValidator: BaseValidator<GetProjectUsersRequest>
    {
        public GetProjectUsersRequestValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Project id is required");
        }
    }
}
