using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class GetAllProjectWorktypesRequestValidator: BaseValidator<GetAllProjectWorktypesRequest>
    {
        public GetAllProjectWorktypesRequestValidator() {
            RuleFor(x => x.ProjectId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Project Id is required");
        }
    }
}
