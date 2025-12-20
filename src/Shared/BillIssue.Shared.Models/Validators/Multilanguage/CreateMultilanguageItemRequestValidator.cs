using BillIssue.Shared.Models.Request.Multilanguage;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Multilanguage
{
    public class CreateMultilanguageItemRequestValidator: BaseValidator<CreateMultilanguageItemRequest>
    {
        public CreateMultilanguageItemRequestValidator()
        {
            RuleFor(x => x.MultilanguageItem).NotNull().WithMessage("Multilanguage item must be provided.");
            When(x => x.MultilanguageItem != null, () =>
            {
                RuleFor(x => x.MultilanguageItem.MultilanguageIndexName)
                    .NotEmpty().WithMessage("Multilanguage index name must be provided.")
                    .MaximumLength(200).WithMessage("Multilanguage index name must not exceed 200 characters.");
                RuleFor(x => x.MultilanguageItem.MultilanguageTranslation)
                    .NotEmpty().WithMessage("Multilanguage translation must be provided.")
                    .MaximumLength(1000).WithMessage("Multilanguage translation must not exceed 1000 characters.");
                RuleFor(x => x.MultilanguageItem.LanguageType)
                    .IsInEnum().WithMessage("Invalid language type specified.");
            });
        }
    }
}
