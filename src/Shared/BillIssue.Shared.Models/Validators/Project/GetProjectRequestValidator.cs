using BillIssue.Shared.Models.Request.Project;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class GetProjectRequestValidator : AbstractValidator<GetProjectRequest>
    {
        public GetProjectRequestValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty().WithMessage("ProjectId is required");
        }
    }
}
