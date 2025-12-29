using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class CreateProjectWorktypeRequestValidator : BaseValidator<CreateProjectWorktypeRequest>
    {
        public CreateProjectWorktypeRequestValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Project id is required");
            
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Project worktype name is required")
                .MaximumLength(200).WithMessage("Project worktype name must not exceed 200 characters");
            
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Project worktype description must not exceed 1000 characters");
        }
    }
}
