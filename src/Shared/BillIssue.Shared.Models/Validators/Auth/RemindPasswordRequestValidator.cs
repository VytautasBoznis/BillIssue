using BillIssue.Shared.Models.Request.Auth;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Auth
{
    public class RemindPasswordRequestValidator: AbstractValidator<RemindPasswordRequest>
    {
        public RemindPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");
        }
    }
}
