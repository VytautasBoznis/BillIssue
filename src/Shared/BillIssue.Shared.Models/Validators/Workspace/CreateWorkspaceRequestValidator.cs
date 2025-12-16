using BillIssue.Shared.Models.Request.Workspace;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Workspace
{
    public class CreateWorkspaceRequestValidator: AbstractValidator<CreateWorkspaceRequest>
    {
        public CreateWorkspaceRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Workspace name is required")
                .MaximumLength(200).WithMessage("Workspace name must not exceed 200 characters");
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Workspace description must not exceed 1000 characters");
        }
    }
}
