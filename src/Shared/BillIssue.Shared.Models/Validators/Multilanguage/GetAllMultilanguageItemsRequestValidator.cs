using BillIssue.Domain.Response.Multilanguage.Dto;
using BillIssue.Shared.Models.Request.Multilanguage;
using FluentValidation;

namespace BillIssue.Shared.Models.Validators.Multilanguage
{
    public class GetAllMultilanguageItemsRequestValidator: AbstractValidator<GetAllMultilanguageItemsRequest>
    {
        public GetAllMultilanguageItemsRequestValidator()
        {
        }
    }
}
