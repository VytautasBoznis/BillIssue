using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class RemoveProjectRequestValidator : BaseValidator<RemoveProjectRequest>
    {
        public RemoveProjectRequestValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty()
                .WithMessage("ProjectId is required.")
                .Must(id => id != Guid.Empty)
                .WithMessage("ProjectId must be a valid non-empty GUID.");
        }
    }
}
