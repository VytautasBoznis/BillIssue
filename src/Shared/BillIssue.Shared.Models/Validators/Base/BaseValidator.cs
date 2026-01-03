using BillIssue.Shared.Models.Request.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Base
{
    public class BaseValidator<T>: AbstractValidator<T> where T : AuthenticatedRequest
    {
        public BaseValidator()
        {
            RuleFor(x => x.SessionUserData)
                .NotNull()
                .When(x => !x.CreatedFromController)
                .WithMessage("SessionUserData must be provided when the request was not created from a controller.");
        }
    }
}
