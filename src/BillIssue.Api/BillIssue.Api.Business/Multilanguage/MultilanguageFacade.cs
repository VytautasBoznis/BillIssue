using BillIssue.Api.Interfaces.Multilanguage;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Models.Multilanguage;
using BillIssue.Data.Enums;
using BillIssue.Domain.Response.Multilanguage.Dto;
using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace BillIssue.Api.Business.Multilanguage
{
    public class MultilanguageFacade : IMultilanguageFacade
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IDatabaseAsync _redisDBAsync;
        private readonly NpgsqlConnection _dbConnection;
        private readonly TimeSpan MaxTranslationCacheDuration = TimeSpan.FromDays(1);
        private readonly ILogger<MultilanguageFacade> _logger;

        private const string CSVSeparator = ";";
        private const string CSVFileSeparatorHeader = $"sep={CSVSeparator}";
        private const string CSVHeader = "MultilanguageIndexName;MultilanguageTranslatedName;LanguageTypeId";

        public MultilanguageFacade(
            IConnectionMultiplexer redisConnection,
            NpgsqlConnection dbConnection,
            ILogger<MultilanguageFacade> logger
        )
        {
            _logger = logger;
            _redisConnection = redisConnection;
            _dbConnection = dbConnection;
            _redisDBAsync = _redisConnection.GetDatabase();
        }

        #region Private methods
        private async Task BuildMultilanguageCaches()
        {
            IEnumerable<MultilanguageIndex> multilanguageIndexes = await _dbConnection.QueryAsync<MultilanguageIndex>("SELECT * FROM multilanguage_indexes");
            IEnumerable<MultilanguageItem> multilanguageItems = await _dbConnection.QueryAsync<MultilanguageItem>("SELECT * FROM multilanguage_items");
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

        private async Task UpdateOrInsertMultilanguageText(MultilanguageItemDto item)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object> { { "@indexName", item.MultilanguageIndexName } };
            MultilanguageIndex multilanguageIndex = await _dbConnection.QueryFirstAsync<MultilanguageIndex>("SELECT * FROM multilanguage_indexes WHERE multilanguage_index_name = @indexName", dictionary);

            if (multilanguageIndex == null)
            {
                await InsertMultilanguageItem(item);
                return;
            }
            else
            {
                await UpdateMultilanguageItem(item);
                return;
            }
        }

        private async Task UpdateMultilanguageItem(MultilanguageItemDto item)
        {
            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            try
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object> { { "@indexName", item.MultilanguageIndexName } };
                MultilanguageIndex multilanguageIndex = await _dbConnection.QueryFirstAsync<MultilanguageIndex>("SELECT * FROM multilanguage_indexes WHERE multilanguage_index_name = @indexName", dictionary);

                dictionary = new Dictionary<string, object> { { "@indexId", multilanguageIndex.MultilanguageIndexId }, { "@languageTypeId", (int) item.LanguageType } };
                MultilanguageItem multilanguageItem = await _dbConnection.QueryFirstAsync<MultilanguageItem>("SELECT * FROM multilanguage_items WHERE multilanguage_index_id = @indexId AND language_type_id = @languageTypeId", dictionary);

                if (multilanguageItem == null)
                {
                    throw new Exception($"No translation found for specified index '{item.MultilanguageIndexName}', database miss match needs to be manually fixed");
                }

                await using NpgsqlCommand updateItem = new NpgsqlCommand("UPDATE multilanguage_items SET Text = @text, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE multilanguage_item_id = @itemId", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@itemId", multilanguageItem.MultilanguageItemId),
                        new("@text", item.MultilanguageTranslation),
                        new("@modifiedBy", "Mass CSV Import"),
                        new("@modifiedOn", DateTime.Now)
                    }
                };

                await updateItem.ExecuteNonQueryAsync();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to update multilanguage item with id: {item.MultilanguageIndexName} Error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                transaction.Rollback();
            }
        }

        private async Task InsertMultilanguageItem(MultilanguageItemDto item)
        {
            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            try
            {
                await using NpgsqlCommand insertIndex = new NpgsqlCommand("INSERT INTO multilanguage_indexes (multilanguage_index_name, description_mlt_id, created_by) VALUES (@indexName, @description, @createdBy)", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@indexName", item.MultilanguageIndexName),
                        new("@description", "Added by API"),
                        new("@createdBy", "Admin"),
                    }
                };

                await insertIndex.ExecuteNonQueryAsync();

                Dictionary<string, object> dictionary = new Dictionary<string, object> { { "@indexName", item.MultilanguageIndexName } };
                MultilanguageIndex multilanguageIndex = await _dbConnection.QueryFirstAsync<MultilanguageIndex>("SELECT * FROM multilanguage_indexes WHERE multilanguage_index_name = @indexName", dictionary, transaction);

                await using NpgsqlCommand insertTranslation = new NpgsqlCommand("INSERT INTO multilanguage_items (multilanguage_index_id, language_type_id, text, created_by) VALUES (@indexId, @languageTypeId, @text, @createdBy)", _dbConnection, transaction)
                {
                    Parameters =
                    {
                        new("@indexId", multilanguageIndex.MultilanguageIndexId),
                        new("@languageTypeId", (int) item.LanguageType),
                        new("@text", item.MultilanguageTranslation),
                        new("@createdBy", "Admin"),
                    }
                };

                await insertTranslation.ExecuteNonQueryAsync();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to add multilanguage item with id: {item.MultilanguageIndexName} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                transaction.Rollback();
            }
        }

        #endregion

        #region Interface implementation

        public async Task ClearMultilanguageCaches()
        {
            await _redisDBAsync.KeyDeleteAsync(RedisCacheKeys.LanguageDictionryCacheKeyPrefix + LanguageTypeEnum.English);
            await _redisDBAsync.KeyDeleteAsync(RedisCacheKeys.LanguageDictionryCacheKeyPrefix + LanguageTypeEnum.Lithuanian);
        }

        public async Task<List<MultilanguageItemDto>> GetAllMultilanguageItems()
        {
            List<MultilanguageItemDto> multilanguageItemsEn = await GetAllMultilanguageItems(LanguageTypeEnum.English);
            List<MultilanguageItemDto> multilanguageItemsLt = await GetAllMultilanguageItems(LanguageTypeEnum.Lithuanian);

            multilanguageItemsEn.AddRange(multilanguageItemsLt);

            return multilanguageItemsEn;
        }

        public async Task<List<MultilanguageItemDto>> GetAllMultilanguageItems(LanguageTypeEnum languageType)
        {
            var cachedLanguageDictionary = await _redisDBAsync.StringGetAsync(RedisCacheKeys.LanguageDictionryCacheKeyPrefix + languageType);

            if (cachedLanguageDictionary == RedisValue.Null)
            {
                await ClearMultilanguageCaches();
                await BuildMultilanguageCaches();
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

        public async Task<DateTime> GetMultilanguageCacheBuildTime()
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

        public async Task AddMultilanguageItem(MultilanguageItemDto item)
        {
            await InsertMultilanguageItem(item);
        }

        public async Task ImportMultilanguageCsv(Stream fileStream)
        {
            using (StreamReader reader = new(fileStream))
            {
                string fileContent = reader.ReadToEnd();
                string[] csvLines = fileContent.Split('\n');

                List<MultilanguageItemDto> multilanguageItems = [];

                for(int i = 0; i < csvLines.Length; i++)
                {
                    if (i == 0 && csvLines[i].Contains("sep"))
                    {
                        continue;
                    }

                    if (( i == 0 || i == 1 ) && csvLines[i] == CSVHeader)
                    {
                        continue;
                    }

                    string[] multilanguegaItemAsStringSplit = csvLines[i].Split(CSVSeparator);

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

                foreach(var item in multilanguageItems)
                {
                    await UpdateOrInsertMultilanguageText(item);
                }
            }
        }

        public async Task<byte[]> GetAllTranslationsInCSV()
        {
            IEnumerable<MultilanguageIndex> multilanguageIndexes = await _dbConnection.QueryAsync<MultilanguageIndex>("SELECT * FROM multilanguage_indexes");
            IEnumerable<MultilanguageItem> multilanguageItems = await _dbConnection.QueryAsync<MultilanguageItem>("SELECT * FROM multilanguage_items");
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

            allMultilanguageItems = allMultilanguageItems.OrderBy(ami => ami.LanguageType).ToList();


            string csvFileContent = CSVFileSeparatorHeader + "\n";
            csvFileContent += CSVHeader + "\n";

            foreach (var mltItem in allMultilanguageItems)
            {
                csvFileContent += $"{mltItem.MultilanguageIndexName}{CSVSeparator}{mltItem.MultilanguageTranslation}{CSVSeparator}{(int)mltItem.LanguageType}\n";
            }

            return Encoding.UTF8.GetBytes(csvFileContent);
        }

        #endregion
    }
}
