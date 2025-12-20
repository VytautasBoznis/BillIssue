using BillIssue.Shared.Models.Request.Multilanguage;
using BillIssue.Shared.Models.Validators.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Multilanguage
{
    public class ModifyMultilanguageItemRequestValidator : BaseValidator<ModifyMultilanguageItemRequest>
    {
        public ModifyMultilanguageItemRequestValidator()
        {
            RuleFor(x => x.MultilanguageItem).NotNull().WithMessage("Multilanguage item data must be provided.");
            When(x => x.MultilanguageItem != null, () =>
            {
                RuleFor(x => x.MultilanguageItem.MultilanguageIndexName)
                    .NotEmpty().WithMessage("Multilanguage index name must not be empty.")
                    .MaximumLength(100).WithMessage("Multilanguage index name must not exceed 100 characters.");
                RuleFor(x => x.MultilanguageItem.LanguageType)
                    .IsInEnum().WithMessage("Invalid language type specified.");
                RuleFor(x => x.MultilanguageItem.MultilanguageTranslation)
                    .NotEmpty().WithMessage("Multilanguage translation must not be empty.")
                    .MaximumLength(1000).WithMessage("Multilanguage translation must not exceed 1000 characters.");
            });
        }
    }
}
