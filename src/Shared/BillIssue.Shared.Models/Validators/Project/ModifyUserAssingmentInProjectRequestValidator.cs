using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class ModifyUserAssingmentInProjectRequestValidator : BaseValidator<ModifyUserAssingmentInProjectRequest>
    {
        public ModifyUserAssingmentInProjectRequestValidator() 
        {
            RuleFor(x => x.ProjectId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Project id is required");
            
            RuleFor(x => x.ProjectUserAssignmentId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Project user assignment is required");
            
            RuleFor(x => x.Role)
                .NotNull()
                .WithMessage("Role must be set");
        }
    }
}
