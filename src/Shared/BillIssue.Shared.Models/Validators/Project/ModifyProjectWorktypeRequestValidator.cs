using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class ModifyProjectWorktypeRequestValidator : BaseValidator<ModifyProjectWorktypeRequest>
    {
        public ModifyProjectWorktypeRequestValidator()
        {
            RuleFor(x => x.ProjectWorktypeId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Project worktype id is required");
            
            RuleFor(x => x.ProjectId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Project id is required");
        }
    }
}
