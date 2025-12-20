using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.Constants;
using BillIssue.Data.Enums;
using BillIssue.Shared.Models.Request.Multilanguage;
using BillIssue.Shared.Models.Response.Multilanguage;
using FluentValidation;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BillIssue.Api.Business.Operations.Multilanguage
{
    public class ClearMultilanguageCachesOperation : BaseOperation<ClearMultilanguageCachesRequest, ClearMultilanguageCachesResponse>
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IDatabaseAsync _redisDBAsync;

        public ClearMultilanguageCachesOperation(
            ILogger<ClearMultilanguageCachesOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<ClearMultilanguageCachesRequest> validator,
            IConnectionMultiplexer redisConnection) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _redisConnection = redisConnection;
            _redisDBAsync = _redisConnection.GetDatabase();
        }

        protected override async Task<ClearMultilanguageCachesResponse> Execute(ClearMultilanguageCachesRequest request, IUnitOfWork unitOfWork)
        {
            await _redisDBAsync.KeyDeleteAsync(RedisCacheKeys.LanguageDictionryCacheKeyPrefix + LanguageTypeEnum.English);
            await _redisDBAsync.KeyDeleteAsync(RedisCacheKeys.LanguageDictionryCacheKeyPrefix + LanguageTypeEnum.Lithuanian);

            return new ClearMultilanguageCachesResponse();
        }
    
    }
}
