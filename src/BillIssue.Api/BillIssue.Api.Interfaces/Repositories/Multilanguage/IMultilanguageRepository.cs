using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.Models.Multilanguage;
using BillIssue.Data.Enums;

namespace BillIssue.Api.Interfaces.Repositories.Multilanguage
{
    public interface IMultilanguageRepository
    {
        Task<List<MultilanguageIndex>> GetAllMultilanguageIndexes(IUnitOfWork unitOfWork);
        Task<List<MultilanguageItem>> GetAllMultilanguageItems(IUnitOfWork unitOfWork);

        Task<MultilanguageIndex?> GetMultilanguageIndexById(Guid multilanguageIndexId, IUnitOfWork unitOfWork);
        Task<MultilanguageIndex?> GetMultilanguageIndexByName(string multilanguageIndexName, IUnitOfWork unitOfWork);
        Task<MultilanguageItem?> GetMultilanguageItemByIndexIdAndLanguage(Guid multilanguageIndexId, LanguageTypeEnum languageTypeId, IUnitOfWork unitOfWork);

        Task CreateMultilanguageIndex(string multilanguageIndex, IUnitOfWork unitOfWork);
        Task CreateMultilanguageItem(MultilanguageItem multilanguageItem, IUnitOfWork unitOfWork);
        Task ModifyMultilanguageItem(MultilanguageItem newMultilanguageItem, IUnitOfWork unitOfWork);
    }
}
