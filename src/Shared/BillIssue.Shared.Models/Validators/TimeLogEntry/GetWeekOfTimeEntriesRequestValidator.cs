using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.TimeLogEntry
{
    public class GetWeekOfTimeEntriesRequestValidator : BaseValidator<GetWeekOfTimeEntriesRequest>
    {
        public GetWeekOfTimeEntriesRequestValidator() {

            RuleFor(x => x.WorkspaceId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Workspace Id is required");
            
            RuleFor(x => x.TargetDay)
                .NotNull()
                .NotEmpty()
                .WithMessage("Target day must be set");
        }
    }
}
