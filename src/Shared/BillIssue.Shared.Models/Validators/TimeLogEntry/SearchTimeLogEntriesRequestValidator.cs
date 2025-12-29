using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.TimeLogEntry
{
    public class SearchTimeLogEntriesRequestValidator : BaseValidator<SearchTimeLogEntriesRequest>
    {
        public SearchTimeLogEntriesRequestValidator() 
        {
            RuleFor(x => x.WorkspaceId).NotNull().NotEmpty().WithMessage("Workspace id is required");
        }

    }
}
