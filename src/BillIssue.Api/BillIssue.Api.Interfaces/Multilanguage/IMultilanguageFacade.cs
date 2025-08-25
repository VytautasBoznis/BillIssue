using BillIssue.Data.Enums;
using BillIssue.Domain.Response.Multilanguage.Dto;

namespace BillIssue.Api.Interfaces.Multilanguage
{
    public interface IMultilanguageFacade
    {
        Task ClearMultilanguageCaches();

        Task<List<MultilanguageItemDto>> GetAllMultilanguageItems();

        Task<List<MultilanguageItemDto>> GetAllMultilanguageItems(LanguageTypeEnum languageType);

        Task<DateTime> GetMultilanguageCacheBuildTime();

        Task<byte[]> GetAllTranslationsInCSV();

        Task AddMultilanguageItem(MultilanguageItemDto item);

        Task ImportMultilanguageCsv(Stream fileStream);
    }
}
