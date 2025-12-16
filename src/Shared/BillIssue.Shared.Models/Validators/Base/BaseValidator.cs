using BillIssue.Shared.Models.Request.Base;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Base
{
    public class BaseValidator<T>: AbstractValidator<T> where T : AuthenticatedRequest
    {
        public BaseValidator()
        {
            RuleFor(x => x.SessionUserData).NotNull().WithMessage("Session user data must be provided.");
        }
    }
}
