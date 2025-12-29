using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.TimeLogEntry
{
    public class ModifyTimeLogEntryRequestValidator : BaseValidator<ModifyTimeLogEntryRequest>
    {
        public ModifyTimeLogEntryRequestValidator() 
        {
            RuleFor(x => x.TimeLogEntryId)
                .NotNull()
                .NotEmpty()
                .WithMessage("TimeLog Entry id is required");

            RuleFor(x => x.ProjectId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Project id is required");

            RuleFor(x => x.ProjectWorktypeId)
                .NotNull()
                .NotEmpty()
                .WithMessage("Project Worktype Id id is required");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Time log entry title must not exceed 200 characters");

            RuleFor(x => x.WorkDescription)
                .MaximumLength(1000).WithMessage("Time log entry work description must not exceed 1000 characters");
        }
    }
}
