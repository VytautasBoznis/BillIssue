using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class CreateProjectRequestValidator: BaseValidator<CreateProjectRequest>
    {
        public CreateProjectRequestValidator()
        {
            RuleFor(RuleFor => RuleFor.WorkspaceId)
                .NotEmpty().WithMessage("WorkspaceId is required");
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Project name is required")
                .MaximumLength(200).WithMessage("Project name must not exceed 200 characters");
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Project description must not exceed 1000 characters");
        }
    }
}
