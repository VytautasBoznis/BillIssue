using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Operations.Multilanguage;
using BillIssue.Api.Controllers.Base;
using BillIssue.Api.Models.Constants;
using BillIssue.Data.Enums;
using BillIssue.Domain.Response.Multilanguage.Dto;
using BillIssue.Shared.Models.Authentication;
using BillIssue.Shared.Models.Request.Multilanguage;
using BillIssue.Shared.Models.Response.Multilanguage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillIssue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MultilanguageController : BaseController
    {
        private OperationFactory _operationFactory;

        public MultilanguageController(
            ILogger<MultilanguageController> logger,
            OperationFactory operationFactory) : base(logger)
        {
            _operationFactory = operationFactory;
        }

        [HttpGet("GetLanguageDictionary/{languageTypeEnum}")]
        public async Task<IActionResult> GetLanguageDictionary([FromRoute] LanguageTypeEnum languageTypeEnum)
        {
            GetAllMultilanguageItemsRequest request = new GetAllMultilanguageItemsRequest { LanguageTypeEnum = languageTypeEnum };

            GetMultilanguageItemsResponse response = await _operationFactory
                                                               .Get<GetAllMultilanguageItemsOperation>()
                                                               .Run(request);
            return Ok(response);
        }

        [HttpGet("GetAllDictionary")]
        public async Task<IActionResult> GetAllMultilanguageItems()
        {
            GetAllMultilanguageItemsRequest request = new GetAllMultilanguageItemsRequest { LanguageTypeEnum = LanguageTypeEnum.All };

            GetMultilanguageItemsResponse response = await _operationFactory
                                                               .Get<GetAllMultilanguageItemsOperation>()
                                                               .Run(request);
            return Ok(response);
        }

        [HttpPost("AddTranslation")]
        [Authorize(Policy = AuthConstants.AdminRequiredPolicyName)]
        public async Task<IActionResult> CreateMultilanguageTranslation(MultilanguageItemDto multilanguageItem)
        {
            SessionUserData sessionUserData = GetSessionModelFromJwt();

            CreateMultilanguageItemResponse response = await _operationFactory
                                                               .Get<CreateMultilanguageItemOperation>()
                                                               .Run(new CreateMultilanguageItemRequest
                                                               {
                                                                   MultilanguageItem = multilanguageItem,
                                                                   SessionUserData = sessionUserData,
                                                               });

            return Ok(response);
        }

        [HttpPost("ImportMultilanguageCSV")]
        [Authorize(Policy = AuthConstants.AdminRequiredPolicyName)]
        public async Task<IActionResult> ImportMultilanguageTranslations([FromForm] IFormFile multilanguageCsvFile)
        {
            Stream multilanguageCsvFileStream = multilanguageCsvFile.OpenReadStream();
            ImportMultilanguageCsvResponse response = await _operationFactory
                                                               .Get<ImportMultilanguageCsvOperation>()
                                                               .Run(new ImportMultilanguageCsvRequest
                                                               {
                                                                   FileStream = multilanguageCsvFileStream,
                                                                   SessionUserData = GetSessionModelFromJwt()
                                                               });

            ClearMultilanguageCachesResponse clearCacheResponse = await _operationFactory
                                                               .Get<ClearMultilanguageCachesOperation>()
                                                               .Run(new ClearMultilanguageCachesRequest());

            return Ok(response);
        }

        [HttpGet("GetAllTranslationsInCSV")]
        [Authorize(Policy = AuthConstants.AdminRequiredPolicyName)]
        public async Task<FileResult> GetAllTranslationsInCSV()
        {
            GetAllTranslationsInCsvResponse translationCsvResponse = await _operationFactory
                                                                       .Get<GetAllTranslationsInCsvOperation>()
                                                                       .Run(new GetAllTranslationsInCsvRequest());

            return File(translationCsvResponse.CsvContentBytes, "text/csv", MultilanguageConstants.CSVFilename);
        }
    }
}
