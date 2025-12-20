using BillIssue.Domain.Response.Multilanguage.Dto;
using BillIssue.Shared.Models.Response.Base;

namespace BillIssue.Shared.Models.Response.Multilanguage
{
    public class GetMultilanguageItemsResponse: BaseResponse
    {
        public DateTime MultilanguageCacheBuildTime { get; set; }
        public List<MultilanguageItemDto> LanguageDictionary { get; set; }
    }
}
