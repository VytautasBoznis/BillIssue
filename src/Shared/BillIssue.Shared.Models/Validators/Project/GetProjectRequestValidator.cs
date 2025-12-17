using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class GetProjectRequestValidator : BaseValidator<GetProjectRequest>
    {
        public GetProjectRequestValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty().WithMessage("ProjectId is required");
        }
    }
}
