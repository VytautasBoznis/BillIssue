using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Project
{
    public class AddUserAssignmentToProjectRequestValidator : BaseValidator<AddUserAssignmentToProjectRequest>
    {
        public AddUserAssignmentToProjectRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotNull()
                .NotEmpty()
                .WithMessage("User Id is required");
            
            RuleFor(x => x.ProjectId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Project id is required");
            
            RuleFor(x => x.Role)
                .NotNull()
                .NotEmpty()
                .WithMessage("Role is required");
        }
    }
}
