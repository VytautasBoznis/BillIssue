using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Multilanguage;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Models.Multilanguage;
using BillIssue.Data.Enums;
using BillIssue.Domain.Response.Multilanguage.Dto;
using BillIssue.Shared.Models.Request.Multilanguage;
using BillIssue.Shared.Models.Response.Multilanguage;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Data;

namespace BillIssue.Api.Business.Operations.Multilanguage
{
    public class GetAllMultilanguageItemsOperation : BaseOperation<GetAllMultilanguageItemsRequest, GetMultilanguageItemsResponse>
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IDatabaseAsync _redisDBAsync;

        private readonly IMultilanguageRepository _multilanguageRepository;

        public GetAllMultilanguageItemsOperation(
            ILogger<GetAllMultilanguageItemsOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<GetAllMultilanguageItemsRequest> validator,
            IConnectionMultiplexer redisConnection,
            IMultilanguageRepository multilanguageRepository
            ) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _redisConnection = redisConnection;
            _redisDBAsync = _redisConnection.GetDatabase();
            _multilanguageRepository = multilanguageRepository;
        }

        protected override async Task<GetMultilanguageItemsResponse> Execute(GetAllMultilanguageItemsRequest request, IUnitOfWork unitOfWork)
        {
            List<MultilanguageItemDto> languageDictionary;

            switch (request.LanguageTypeEnum)
            {
                case LanguageTypeEnum.English:
                case LanguageTypeEnum.Lithuanian:
                    {
                        languageDictionary = await GetAllMultilanguageItems(request.LanguageTypeEnum, unitOfWork);
                        break;
                    }
                case LanguageTypeEnum.All:
                    {
                        languageDictionary = await GetAllMultilanguageItems(LanguageTypeEnum.English, unitOfWork);
                        List<MultilanguageItemDto> multilanguageItemsLt = await GetAllMultilanguageItems(LanguageTypeEnum.Lithuanian, unitOfWork);

                        languageDictionary.AddRange(multilanguageItemsLt);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(request.LanguageTypeEnum), "Unsupported language type");
            }

            return new GetMultilanguageItemsResponse {
                LanguageDictionary = languageDictionary,
                MultilanguageCacheBuildTime = await GetMultilanguageCacheBuildTime()
            };
        }

        private async Task<List<MultilanguageItemDto>> GetAllMultilanguageItems(LanguageTypeEnum languageType, IUnitOfWork unitOfWork)
        {
            var cachedLanguageDictionary = await _redisDBAsync.StringGetAsync(RedisCacheKeys.LanguageDictionryCacheKeyPrefix + languageType);

            if (cachedLanguageDictionary == RedisValue.Null)
            {
                ClearMultilanguageCachesResponse clearCacheResponse = await _operationFactory
                                                                   .Get<ClearMultilanguageCachesOperation>(typeof(ClearMultilanguageCachesOperation))
                                                                   .Run(new ClearMultilanguageCachesRequest());

                await BuildMultilanguageCaches(unitOfWork);
                cachedLanguageDictionary = await _redisDBAsync.StringGetAsync(RedisCacheKeys.LanguageDictionryCacheKeyPrefix + languageType);
            }

            if (cachedLanguageDictionary == RedisValue.Null)
            {
                _logger.LogError("Failed to load multilanguage data with rebuild");
                //TODO: Probably add a special exception type for this
                throw new Exception("Failed to load multilanguage data with rebuild");
            }

            List<MultilanguageItemDto> multilanguageData = JsonConvert.DeserializeObject<List<MultilanguageItemDto>>(cachedLanguageDictionary.ToString());

            return multilanguageData == null ? new List<MultilanguageItemDto>() : multilanguageData;
        }

        private async Task BuildMultilanguageCaches(IUnitOfWork unitOfWork)
        {
            List<MultilanguageIndex> multilanguageIndexes = await _multilanguageRepository.GetAllMultilanguageIndexes(unitOfWork);
            List<MultilanguageItem> multilanguageItems = await _multilanguageRepository.GetAllMultilanguageItems(unitOfWork);
            List<MultilanguageItemDto> allMultilanguageItems = new List<MultilanguageItemDto>();

            foreach (MultilanguageIndex index in multilanguageIndexes)
            {
                List<MultilanguageItem> translations = multilanguageItems.Where(mt => mt.MultilanguageIndexId == index.MultilanguageIndexId).ToList();

                foreach (MultilanguageItem translation in translations)
                {
                    MultilanguageItemDto newTranslationItem = new MultilanguageItemDto
                    {
                        MultilanguageIndexName = index.MultilanguageIndexName,
                        MultilanguageTranslation = translation.Text,
                        LanguageType = translation.LanguageTypeId
                    };

                    allMultilanguageItems.Add(newTranslationItem);
                }
            }

            //Add english
            List<MultilanguageItemDto> itemsForStorate = allMultilanguageItems.Where(ami => ami.LanguageType == LanguageTypeEnum.English).ToList();
            await _redisDBAsync.StringSetAsync(RedisCacheKeys.LanguageDictionryCacheKeyPrefix + LanguageTypeEnum.English, JsonConvert.SerializeObject(itemsForStorate));

            //Add Lithuanian
            itemsForStorate = allMultilanguageItems.Where(ami => ami.LanguageType == LanguageTypeEnum.Lithuanian).ToList();
            await _redisDBAsync.StringSetAsync(RedisCacheKeys.LanguageDictionryCacheKeyPrefix + LanguageTypeEnum.Lithuanian, JsonConvert.SerializeObject(itemsForStorate));

            //Add Cache build timestamp
            await _redisDBAsync.StringSetAsync(RedisCacheKeys.LanguageDictionryCacheBuildTimeKey, DateTime.Now.ToString());
        }

        private async Task<DateTime> GetMultilanguageCacheBuildTime()
        {
            var cacheCreateTimeString = await _redisDBAsync.StringGetAsync(RedisCacheKeys.LanguageDictionryCacheBuildTimeKey);

            if (cacheCreateTimeString == RedisValue.Null)
            {
                _logger.LogError("No multilanguage cache built");
                throw new Exception("No multilanguage cache built");
            }

            DateTime parsedDateTime;

            if (!DateTime.TryParse(cacheCreateTimeString, out parsedDateTime))
            {
                _logger.LogError($"Failed to parse cache create time, the value: {cacheCreateTimeString}");
                throw new Exception("Failed to parse cache create time");
            }

            return parsedDateTime;
        }
    }
}
