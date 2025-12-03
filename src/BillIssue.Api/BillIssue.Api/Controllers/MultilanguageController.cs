using BillIssue.Api.ActionFilters;
using BillIssue.Api.Controllers.Base;
using BillIssue.Api.Interfaces.Multilanguage;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Data.Enums;
using BillIssue.Domain.Response.Multilanguage.Dto;
using BillIssue.Shared.Models.Response.Multilanguage;
using Microsoft.AspNetCore.Mvc;

namespace BillIssue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MultilanguageController : BaseController
    {
        private readonly IMultilanguageFacade _multilanguageFacade;

        public MultilanguageController(IMultilanguageFacade multilanguageFacade, ILogger<MultilanguageController> logger) : base(logger)
        {
            _multilanguageFacade = multilanguageFacade;
        }

        [HttpGet("GetLanguageDictionary/{languageTypeEnum}")]
        public async Task<GetMultilanguageDictionyResponse> GetLanguageDictionary([FromRoute] LanguageTypeEnum languageTypeEnum)
        {
            return new GetMultilanguageDictionyResponse
            {
                LanguageDictionary = await _multilanguageFacade.GetAllMultilanguageItems(languageTypeEnum),
                MultilanguageCacheBuildTime = await _multilanguageFacade.GetMultilanguageCacheBuildTime()
            };
        }

        [HttpGet("GetAllDictionary")]
        public async Task<GetMultilanguageDictionyResponse> GetAllMultilanguageItems()
        {
            return new GetMultilanguageDictionyResponse
            {
                LanguageDictionary = await _multilanguageFacade.GetAllMultilanguageItems(),
                MultilanguageCacheBuildTime = await _multilanguageFacade.GetMultilanguageCacheBuildTime()
            };
        }

        [HttpPost("AddTranslation")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.Admin])]
        public async Task CreateMultilanguageTranslation(MultilanguageItemDto multilanguageItem)
        {
            await _multilanguageFacade.AddMultilanguageItem(multilanguageItem);
        }

        [HttpPost("ImportMultilanguageCSV")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.Admin])]
        public async Task<IActionResult> ImportMultilanguageTranslations([FromForm] IFormFile multilanguageCsvFile)
        {
            Stream multilanguageCsvFileStream = multilanguageCsvFile.OpenReadStream();
            await _multilanguageFacade.ImportMultilanguageCsv(multilanguageCsvFileStream);
            await _multilanguageFacade.ClearMultilanguageCaches();

            return Ok();
        }

        [HttpGet("GetAllTranslationsInCSV")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.Admin])]
        public async Task<FileResult> GetAllTranslationsInCSV()
        {
            string fileName = "AllMultilanguageItems.csv";
            byte[] fileBytes = await _multilanguageFacade.GetAllTranslationsInCSV();

            return File(fileBytes, "text/csv", fileName);
        }
    }
}
