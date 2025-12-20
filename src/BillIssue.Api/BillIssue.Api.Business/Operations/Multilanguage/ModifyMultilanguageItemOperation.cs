using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Multilanguage;
using BillIssue.Api.Models.Models.Multilanguage;
using BillIssue.Shared.Models.Request.Multilanguage;
using BillIssue.Shared.Models.Response.Multilanguage;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BillIssue.Api.Business.Operations.Multilanguage
{
    public class ModifyMultilanguageItemOperation : BaseOperation<ModifyMultilanguageItemRequest, ModifyMultilanguageItemResponse>
    {
        private readonly IMultilanguageRepository _multilanguageRepository;

        public ModifyMultilanguageItemOperation(
            ILogger<ModifyMultilanguageItemOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<ModifyMultilanguageItemRequest> validator,
            IMultilanguageRepository multilanguageRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _multilanguageRepository = multilanguageRepository;
        }

        protected override async Task<ModifyMultilanguageItemResponse> Execute(ModifyMultilanguageItemRequest request, IUnitOfWork unitOfWork)
        {
            try
            {
                await unitOfWork.BeginTransactionAsync();

                MultilanguageIndex multilanguageIndex = await _multilanguageRepository.GetMultilanguageIndexByName(request.MultilanguageItem.MultilanguageIndexName, unitOfWork);
                MultilanguageItem multilanguageItem = await _multilanguageRepository.GetMultilanguageItemByIndexIdAndLanguage(multilanguageIndex.MultilanguageIndexId, request.MultilanguageItem.LanguageType, unitOfWork);

                if (multilanguageItem == null)
                {
                    throw new Exception($"No translation found for specified index '{multilanguageIndex.MultilanguageIndexName}', database miss match needs to be manually fixed");
                }
                
                multilanguageItem.Text =  request.MultilanguageItem.MultilanguageTranslation;

                await _multilanguageRepository.ModifyMultilanguageItem(multilanguageItem, unitOfWork);

                if (request.StopTransactionCommit)
                {
                    await unitOfWork.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to update multilanguage item with id: {request.MultilanguageItem.MultilanguageIndexName} Error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                await unitOfWork.RollbackAsync();
            }

            return new ModifyMultilanguageItemResponse();
        }
    }
}
