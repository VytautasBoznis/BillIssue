using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class GetProjectWorktypeRequestValidator : BaseValidator<GetProjectWorktypeRequest>
    {
        public GetProjectWorktypeRequestValidator()
        {
            RuleFor(x => x.ProjectWorktypeId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Project worktype id is required.");
        }
    }
}
