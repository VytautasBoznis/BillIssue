using BillIssue.Shared.Models.Request.Multilanguage;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Multilanguage
{
    public class ImportMultilanguageCsvRequestValidator: BaseValidator<ImportMultilanguageCsvRequest>
    {
        public ImportMultilanguageCsvRequestValidator()
        {
            RuleFor(x => x.FileStream)
                .NotNull().NotEmpty().WithMessage("File content must not be empty.");
        }
    }
}
