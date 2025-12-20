using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Multilanguage;
using BillIssue.Api.Models.Models.Base;
using BillIssue.Api.Models.Models.Multilanguage;
using BillIssue.Data.Enums;
using BillIssue.Domain.Response.Multilanguage.Dto;
using Dapper;
using Npgsql;
using System.Collections.Generic;
using System.Transactions;

namespace BillIssue.Api.Business.Repositories.Multilanguage
{
    public class MultilanguageRepository : IMultilanguageRepository
    {

        public async Task<List<MultilanguageIndex>> GetAllMultilanguageIndexes(IUnitOfWork unitOfWork)
        {
            IEnumerable<MultilanguageIndex> multilanguageIndexes = await unitOfWork.Connection.QueryAsync<MultilanguageIndex>("SELECT * FROM multilanguage_indexes");

            return multilanguageIndexes.ToList();
        }

        public async Task<List<MultilanguageItem>> GetAllMultilanguageItems(IUnitOfWork unitOfWork)
        {
            IEnumerable<MultilanguageItem> multilanguageItems = await unitOfWork.Connection.QueryAsync<MultilanguageItem>("SELECT * FROM multilanguage_items");

            return multilanguageItems.ToList();
        }
        public async Task<MultilanguageIndex?> GetMultilanguageIndexById(Guid multilanguageIndexId, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@multiLanguageIndexId", multilanguageIndexId } };

            MultilanguageIndex? multilanguageIndex = await unitOfWork.Connection.QueryFirstOrDefaultAsync<MultilanguageIndex>("SELECT * FROM multilanguage_indexes WHERE multilanguage_index_id = @multiLanguageIndexId", dictionary);

            return multilanguageIndex;
        }

        public async Task<MultilanguageIndex> GetMultilanguageIndexByName(string multilanguageIndexName, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@multiLanguageIndexName", multilanguageIndexName } };

            MultilanguageIndex? multilanguageIndex = await unitOfWork.Connection.QueryFirstOrDefaultAsync<MultilanguageIndex>("SELECT * FROM multilanguage_indexes WHERE multilanguage_index_name = @multiLanguageIndexName", dictionary);

            return multilanguageIndex;
        }

        public async Task CreateMultilanguageIndex(string multilanguageIndex, IUnitOfWork unitOfWork)
        {
            try
            {
                await using NpgsqlCommand insertIndex = new NpgsqlCommand("INSERT INTO multilanguage_indexes (multilanguage_index_name, description_mlt_id, created_by) VALUES (@indexName, @description, @createdBy)", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@indexName", multilanguageIndex),
                        new("@description", "Added by API"),
                        new("@createdBy", "Admin"),
                    }
                };

                await insertIndex.ExecuteNonQueryAsync();
            }
            catch
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task CreateMultilanguageItem(MultilanguageItem multilanguageItem, IUnitOfWork unitOfWork)
        {
            try
            {
                await using NpgsqlCommand insertTranslation = new NpgsqlCommand("INSERT INTO multilanguage_items (multilanguage_index_id, language_type_id, text, created_by) VALUES (@indexId, @languageTypeId, @text, @createdBy)", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                        {
                            new("@indexId", multilanguageItem.MultilanguageIndexId),
                            new("@languageTypeId", (int) multilanguageItem.LanguageTypeId),
                            new("@text", multilanguageItem.Text),
                            new("@createdBy", "Admin"),
                        }
                };

                await insertTranslation.ExecuteNonQueryAsync();
            }
            catch
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<MultilanguageItem?> GetMultilanguageItemByIndexIdAndLanguage(Guid multilanguageIndexId, LanguageTypeEnum languageTypeId, IUnitOfWork unitOfWork)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object> { { "@indexId", multilanguageIndexId }, { "@languageTypeId", (int)languageTypeId } };
            MultilanguageItem? multilanguageItem = await unitOfWork.Connection.QueryFirstOrDefaultAsync<MultilanguageItem>("SELECT * FROM multilanguage_items WHERE multilanguage_index_id = @indexId AND language_type_id = @languageTypeId", dictionary);

            return multilanguageItem;
        }

        public async Task ModifyMultilanguageItem(MultilanguageItem newMultilanguageItem, IUnitOfWork unitOfWork)
        {
            await using NpgsqlCommand updateItem = new NpgsqlCommand("UPDATE multilanguage_items SET Text = @text, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE multilanguage_item_id = @itemId", unitOfWork.Connection, unitOfWork.Transaction)
            {
                Parameters =
                    {
                        new("@itemId", newMultilanguageItem.MultilanguageItemId),
                        new("@text", newMultilanguageItem.Text),
                        new("@modifiedBy", "Mass CSV Import"),
                        new("@modifiedOn", DateTime.Now)
                    }
            };

            await updateItem.ExecuteNonQueryAsync();
        }
    }
}
