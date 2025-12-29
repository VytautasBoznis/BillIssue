using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.TimeLogEntry
{
    public class GetTimeLogEntryRequestValidator : BaseValidator<GetTimeLogEntryRequest>
    {
        public GetTimeLogEntryRequestValidator()
        {
            RuleFor(x => x.TimeLogEntryId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Time log entry id is required");
        }
    }
}
