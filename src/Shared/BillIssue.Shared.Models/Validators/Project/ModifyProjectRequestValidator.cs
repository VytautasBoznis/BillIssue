using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class ModifyProjectRequestValidator : BaseValidator<ModifyProjectRequest>
    {
        public ModifyProjectRequestValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty()
                .WithMessage("ProjectId is required.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(100)
                .WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters.");
        }
    }
}
