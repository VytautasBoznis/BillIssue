using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Multilanguage;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Models.Multilanguage;
using BillIssue.Data.Enums;
using BillIssue.Domain.Response.Multilanguage.Dto;
using BillIssue.Shared.Models.Authentication;
using BillIssue.Shared.Models.Request.Multilanguage;
using BillIssue.Shared.Models.Response.Multilanguage;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection.PortableExecutable;

namespace BillIssue.Api.Business.Operations.Multilanguage
{
    public class ImportMultilanguageCsvOperation : BaseOperation<ImportMultilanguageCsvRequest, ImportMultilanguageCsvResponse>
    {
        private readonly IMultilanguageRepository _multilanguageRepository;

        public ImportMultilanguageCsvOperation(
            ILogger<ImportMultilanguageCsvOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<ImportMultilanguageCsvRequest> validator,
            IMultilanguageRepository multilanguageRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _multilanguageRepository = multilanguageRepository;
        }

        protected override async Task<ImportMultilanguageCsvResponse> Execute(ImportMultilanguageCsvRequest request, IUnitOfWork unitOfWork)
        {
            try
            {
                await unitOfWork.BeginTransactionAsync();

                using (StreamReader reader = new(request.FileStream))
                {
                    string fileContent = reader.ReadToEnd();
                    string[] csvLines = fileContent.Split('\n');

                    List<MultilanguageItemDto> multilanguageItems = [];

                    for (int i = 0; i < csvLines.Length; i++)
                    {
                        if (i == 0 && csvLines[i].Contains("sep"))
                        {
                            continue;
                        }

                        if ((i == 0 || i == 1) && csvLines[i] == MultilanguageConstants.CSVHeader)
                        {
                            continue;
                        }

                        string[] multilanguegaItemAsStringSplit = csvLines[i].Split(MultilanguageConstants.CSVSeparator);

                        if (multilanguegaItemAsStringSplit.Length > 1)
                        {
                            multilanguageItems.Add(new MultilanguageItemDto
                            {
                                MultilanguageIndexName = multilanguegaItemAsStringSplit[0],
                                MultilanguageTranslation = multilanguegaItemAsStringSplit[1],
                                LanguageType = (LanguageTypeEnum)Enum.Parse(typeof(LanguageTypeEnum), multilanguegaItemAsStringSplit[2])
                            });
                        }
                    }

                    foreach (var item in multilanguageItems)
                    {
                        await UpdateOrInsertMultilanguageText(item, request.SessionUserData, unitOfWork);
                    }
                }

                await unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to import multilanguage CSV file. Error: {ex.Message} \n\n StackTrace: {ex.StackTrace}");
                await unitOfWork.RollbackAsync();
            }

            return new ImportMultilanguageCsvResponse();
        }

        private async Task UpdateOrInsertMultilanguageText(MultilanguageItemDto item, SessionUserData sessionUserData, IUnitOfWork unitOfWork)
        {
            MultilanguageIndex multilanguageIndex = await _multilanguageRepository.GetMultilanguageIndexByName(item.MultilanguageIndexName, unitOfWork);

            if (multilanguageIndex == null)
            {
                CreateMultilanguageItemResponse createResponse = await _operationFactory
                                                                           .Get<CreateMultilanguageItemOperation>(typeof(CreateMultilanguageItemOperation))
                                                                           .Run(new CreateMultilanguageItemRequest
                                                                           {
                                                                               MultilanguageItem = item,
                                                                               SessionUserData = sessionUserData,
                                                                               StopTransactionCommit = true,
                                                                           }, unitOfWork);

                return;
            }
            else
            {
                ModifyMultilanguageItemResponse modifyResponse = await _operationFactory
                                                                           .Get<ModifyMultilanguageItemOperation>(typeof(ModifyMultilanguageItemOperation))
                                                                           .Run(new ModifyMultilanguageItemRequest
                                                                           {
                                                                               MultilanguageItem = item,
                                                                               SessionUserData = sessionUserData,
                                                                               StopTransactionCommit = true,
                                                                           }, unitOfWork);
                return;
            }
        }
    }
}
