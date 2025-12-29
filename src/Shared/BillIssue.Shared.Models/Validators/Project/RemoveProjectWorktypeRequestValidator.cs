using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class RemoveProjectWorktypeRequestValidator : BaseValidator<RemoveProjectWorktypeRequest>
    {
        public RemoveProjectWorktypeRequestValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Project Id is required");
            
            RuleFor(x => x.ProjectWorktypeId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Project Worktype Id is required");
        }
    }
}
