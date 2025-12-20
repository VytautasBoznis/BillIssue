using BillIssue.Data.Enums;
using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Multilanguage
{
    public class GetAllMultilanguageItemsRequest: BaseRequest
    {
        public LanguageTypeEnum LanguageTypeEnum { get; set; }
    }
}
