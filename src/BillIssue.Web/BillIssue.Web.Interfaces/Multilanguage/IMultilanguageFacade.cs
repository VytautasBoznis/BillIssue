using BillIssue.Data.Enums;
using BillIssue.Domain.Response.Multilanguage.Dto;

namespace BillIssue.Interfaces.Multilanguage
{
    public interface IMultilanguageFacade
    {
        Task<List<MultilanguageItemDto>> GetAllMultilanguageTextList();
        Task RebuildMultilanguageCaches();
        Task<string> Get(string key, string defaultValue = null, LanguageTypeEnum? languageType = null);
    }
}
