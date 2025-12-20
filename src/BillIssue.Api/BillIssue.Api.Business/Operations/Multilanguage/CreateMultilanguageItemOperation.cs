using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Multilanguage;
using BillIssue.Api.Models.Models.Multilanguage;
using BillIssue.Shared.Models.Request.Multilanguage;
using BillIssue.Shared.Models.Response.Multilanguage;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Multilanguage
{
    public class CreateMultilanguageItemOperation : BaseOperation<CreateMultilanguageItemRequest, CreateMultilanguageItemResponse>
    {
        private IMultilanguageRepository _multilanguageRepository;

        public CreateMultilanguageItemOperation(
            ILogger<CreateMultilanguageItemOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<CreateMultilanguageItemRequest> validator,
            IMultilanguageRepository multilanguageRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _multilanguageRepository = multilanguageRepository;
        }

        protected override async Task<CreateMultilanguageItemResponse> Execute(CreateMultilanguageItemRequest request, IUnitOfWork unitOfWork)
        {
            try
            {
                await unitOfWork.BeginTransactionAsync();

                MultilanguageIndex multilanguageIndex = await _multilanguageRepository.GetMultilanguageIndexByName(request.MultilanguageItem.MultilanguageIndexName, unitOfWork);

                if(multilanguageIndex == null)
                {
                    await _multilanguageRepository.CreateMultilanguageIndex(request.MultilanguageItem.MultilanguageIndexName, unitOfWork);
                    multilanguageIndex = await _multilanguageRepository.GetMultilanguageIndexByName(request.MultilanguageItem.MultilanguageIndexName, unitOfWork);
                }

                await _multilanguageRepository.CreateMultilanguageItem(new MultilanguageItem
                                                                            {
                                                                                MultilanguageIndexId = multilanguageIndex.MultilanguageIndexId,
                                                                                LanguageTypeId = request.MultilanguageItem.LanguageType,
                                                                                Text = request.MultilanguageItem.MultilanguageTranslation,
                                                                                CreatedBy = request.SessionUserData.Email,
                                                                            }, unitOfWork);

                if (request.StopTransactionCommit)
                {
                    await unitOfWork.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to add multilanguage item with id: {request.MultilanguageItem.MultilanguageIndexName} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                await unitOfWork.RollbackAsync();
            }

            return new CreateMultilanguageItemResponse();
        }
    }
}
