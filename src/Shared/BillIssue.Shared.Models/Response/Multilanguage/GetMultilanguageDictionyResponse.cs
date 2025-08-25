using BillIssue.Domain.Response.Multilanguage.Dto;
using BillIssue.Shared.Models.Response.Base;

namespace BillIssue.Shared.Models.Response.Multilanguage
{
    public class GetMultilanguageDictionyResponse: BaseResponse
    {
        public GetMultilanguageDictionyResponse() { }

        public DateTime MultilanguageCacheBuildTime { get; set; }
        public List<MultilanguageItemDto> LanguageDictionary { get; set; }
    }
}
