using BillIssue.Data.Enums;
using BillIssue.Domain.Response.Multilanguage.Dto;
using BillIssue.Interfaces.Multilanguage;
using BillIssue.Shared.Models.Response.Multilanguage;
using BillIssue.Web.Business.RestClient;
using BillIssue.Web.Domain.Constants;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace BillIssue.Web.Business.Multilanguage
{
    public class MultilanguageFacade : IMultilanguageFacade
    {
        private readonly BillIssueApiClient _client;
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private readonly IMemoryCache _memoryCache;
        private readonly bool ForceMultilanguageKeys;

        private const int MaximumCacheLifeInHours = 1;

        public MultilanguageFacade(BillIssueApiClient client, IMemoryCache memoryCache, IConfiguration configuration)
        {
            _client = client;
            _memoryCache = memoryCache;
            _cacheOptions = new MemoryCacheEntryOptions()
                .SetSize(1)
                .SetAbsoluteExpiration(TimeSpan.FromDays(2));

            bool configParseSuccessfull = bool.TryParse(configuration[ConfigurationKeys.ForceMultilanguageKeys], out bool forceKeyConfig);

            if (configParseSuccessfull)
            {
                ForceMultilanguageKeys = forceKeyConfig;
            }
        }

        #region Multilanguage Handling

        public async Task RebuildMultilanguageCaches()
        {
            DropAllMultilanguageCaches();
            await BuildAllMultilanguageCaches();
        }

        private void DropAllMultilanguageCaches()
        {
            Dictionary<LanguageTypeEnum, Dictionary<string, string>> allCulturetsDictionary = _memoryCache.Get<Dictionary<LanguageTypeEnum, Dictionary<string, string>>>(CacheKeys.MultilanguageCacheKey);
            foreach (var culture in allCulturetsDictionary)
            {
                culture.Value?.Clear();
            }
        }

        private async Task BuildAllMultilanguageCaches()
        {
            DropAllMultilanguageCaches();
            List<MultilanguageItemDto> allMultilanguageItems = await GetAllMultilanguageTextList();
            Dictionary<LanguageTypeEnum, Dictionary<string, string>> allMultilanguageCultures = _memoryCache.Get<Dictionary<LanguageTypeEnum, Dictionary<string, string>>>(CacheKeys.MultilanguageCacheKey);

            //English
            Dictionary<string, string> cultureDictionary = allMultilanguageItems.FindAll(amli => amli.LanguageType == LanguageTypeEnum.English).ToDictionary(amli => amli.MultilanguageIndexName, amli => amli.MultilanguageTranslation);
            allMultilanguageCultures.Remove(LanguageTypeEnum.English);
            allMultilanguageCultures.Add(LanguageTypeEnum.English, cultureDictionary);

            //Lithuanian
            cultureDictionary = allMultilanguageItems.FindAll(amli => amli.LanguageType == LanguageTypeEnum.Lithuanian).ToDictionary(amli => amli.MultilanguageIndexName, amli => amli.MultilanguageTranslation);
            allMultilanguageCultures.Remove(LanguageTypeEnum.Lithuanian);
            allMultilanguageCultures.Add(LanguageTypeEnum.Lithuanian, cultureDictionary);

            _memoryCache.Set(CacheKeys.MultilanguageCacheKey, allMultilanguageCultures, _cacheOptions);
        }

        public async Task<List<MultilanguageItemDto>> GetAllMultilanguageTextList()
        {
            GetMultilanguageDictionyResponse multilanguageResponse = await _client.GetAllMultilanguageItems();
            _memoryCache.Set(CacheKeys.MultilanguageCacheCreationTimeKey, DateTime.Now, _cacheOptions);

            return multilanguageResponse.LanguageDictionary;
        }

        private async Task BuildMultilanguageCacheByCulture(LanguageTypeEnum cultureType)
        {
            List<MultilanguageItemDto> allMultilanguageItems = await GetAllMultilanguageTextList();
            List<MultilanguageItemDto> multilanguageItemsForCulture = allMultilanguageItems.FindAll(mlt => mlt.LanguageType == cultureType);
            Dictionary<string, string> cultureDictionary = [];

            foreach (var mli in multilanguageItemsForCulture)
            {
                string translation;
                if (!cultureDictionary.TryGetValue(mli.MultilanguageIndexName, out translation))
                {
                    cultureDictionary.Add(mli.MultilanguageIndexName, mli.MultilanguageTranslation);
                }
            }

            Dictionary<LanguageTypeEnum, Dictionary<string, string>>? allCulturetsDictionary = _memoryCache.Get<Dictionary<LanguageTypeEnum, Dictionary<string, string>>>(CacheKeys.MultilanguageCacheKey);

            if (allCulturetsDictionary == null)
            {
                allCulturetsDictionary = [];
            }

            allCulturetsDictionary.Remove(cultureType);
            allCulturetsDictionary.Add(cultureType, cultureDictionary);
            _memoryCache.Set(CacheKeys.MultilanguageCacheKey, allCulturetsDictionary, _cacheOptions);
        }

        private async Task<Dictionary<string, string>> GetTranslationsByCulture(LanguageTypeEnum cultureEnum)
        {
            Dictionary<LanguageTypeEnum, Dictionary<string, string>>? allCulturetsDictionary = _memoryCache.Get<Dictionary<LanguageTypeEnum, Dictionary<string, string>>>(CacheKeys.MultilanguageCacheKey);
            Dictionary<string, string>? cultureDictionary = null;

            allCulturetsDictionary?.TryGetValue(cultureEnum, out cultureDictionary);

            if (cultureDictionary?.Count > 0)
            {
                return cultureDictionary;//if the culture dictionary is preasent return imideatly
            }

            //else build the culture cache and try again
            await BuildMultilanguageCacheByCulture(cultureEnum);

            allCulturetsDictionary = _memoryCache.Get<Dictionary<LanguageTypeEnum, Dictionary<string, string>>>(CacheKeys.MultilanguageCacheKey);
            allCulturetsDictionary?.TryGetValue(cultureEnum, out cultureDictionary);

            return cultureDictionary;
        }

        private async Task CheckMultilanguageForExpiration()
        {
            DateTime? multilanguageCacheBuildTime = _memoryCache.Get<DateTime?>(CacheKeys.MultilanguageCacheCreationTimeKey);

            if (multilanguageCacheBuildTime.HasValue && multilanguageCacheBuildTime.Value.AddHours(MaximumCacheLifeInHours) < DateTime.Now)
            {
                _memoryCache.Remove(CacheKeys.MultilanguageCacheCreationTimeKey);
                _memoryCache.Remove(CacheKeys.MultilanguageCacheKey);
            }
        }

        #endregion

        public async Task<string> Get(string key, string defaultValue = null, LanguageTypeEnum? languageType = null)
        {
            LanguageTypeEnum usingCultureEnum = LanguageTypeEnum.Unknown;

            if (languageType != null)
            {
                usingCultureEnum = languageType.Value;
            }

            await CheckMultilanguageForExpiration();
            Dictionary<string, string> cultureDictonary = await GetTranslationsByCulture(usingCultureEnum);

            if (ForceMultilanguageKeys)
            {
                return key;
            }

            string translation = null;
            cultureDictonary?.TryGetValue(key, out translation);

            if (string.IsNullOrEmpty(translation))
            {
                return string.IsNullOrEmpty(defaultValue) ? key : defaultValue;
            }

            return translation;
        }
    }
}
