using BillIssue.Data.Enums;
using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Domain.Request.Multilanguage
{
    public class GetDictionaryRequest: BaseRequest
    {
        public GetDictionaryRequest() { }

        public LanguageTypeEnum LanguageType { get; set; }
    }
}
