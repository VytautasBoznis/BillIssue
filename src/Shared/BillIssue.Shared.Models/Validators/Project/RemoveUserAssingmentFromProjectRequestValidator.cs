using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class RemoveUserAssingmentFromProjectRequestValidator : BaseValidator<RemoveUserAssingmentFromProjectRequest>
    {
        public RemoveUserAssingmentFromProjectRequestValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Project Id is required");

            RuleFor(x => x.ProjectUserAssignmentId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Project user assignment id is required");
        }
    }
}
