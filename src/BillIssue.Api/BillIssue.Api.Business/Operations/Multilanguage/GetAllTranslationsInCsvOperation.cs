using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Multilanguage;
using BillIssue.Api.Models.Constants;
using BillIssue.Data.Enums;
using BillIssue.Shared.Models.Request.Multilanguage;
using BillIssue.Shared.Models.Response.Multilanguage;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Reflection.PortableExecutable;
using System.Text;

namespace BillIssue.Api.Business.Operations.Multilanguage
{
    public class GetAllTranslationsInCsvOperation : BaseOperation<GetAllTranslationsInCsvRequest, GetAllTranslationsInCsvResponse>
    {
        private readonly IMultilanguageRepository _multilanguageRepository;

        public GetAllTranslationsInCsvOperation(
                ILogger<GetAllTranslationsInCsvOperation> logger,
                IUnitOfWorkFactory unitOfWorkFactory,
                OperationFactory operationFactory,
                IValidator<GetAllTranslationsInCsvRequest> validator,
                IMultilanguageRepository multilanguageRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _multilanguageRepository = multilanguageRepository;
        }

        protected override async Task<GetAllTranslationsInCsvResponse> Execute(GetAllTranslationsInCsvRequest request, IUnitOfWork unitOfWork)
        {
            GetAllMultilanguageItemsRequest getAllMultilanguageItemsRequest = new GetAllMultilanguageItemsRequest
            {
                LanguageTypeEnum = LanguageTypeEnum.All
            };  

            GetMultilanguageItemsResponse allMultilanguageItemResponse = await _operationFactory
                                                                                   .Get<GetAllMultilanguageItemsOperation>()
                                                                                   .Run(getAllMultilanguageItemsRequest, unitOfWork);

            allMultilanguageItemResponse.LanguageDictionary = allMultilanguageItemResponse.LanguageDictionary.OrderBy(ami => ami.LanguageType).ToList();

            string csvFileContent = MultilanguageConstants.CSVFileSeparatorHeader + "\n";
            csvFileContent += MultilanguageConstants.CSVHeader + "\n";

            foreach (var mltItem in allMultilanguageItemResponse.LanguageDictionary)
            {
                csvFileContent += $"{mltItem.MultilanguageIndexName}{MultilanguageConstants.CSVSeparator}{mltItem.MultilanguageTranslation}{MultilanguageConstants.CSVSeparator}{(int)mltItem.LanguageType}\n";
            }

            return new GetAllTranslationsInCsvResponse
            {
                CsvContentBytes = Encoding.UTF8.GetBytes(csvFileContent)
            };
        }
    }
}
