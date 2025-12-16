using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.ConfigurationOptions;
using BillIssue.Api.Models.Models.Base;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BillIssue.Api.Business.Base
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly DatabaseOptions databaseOptions;

        public UnitOfWorkFactory(IOptions<DatabaseOptions> databaseOption)
        {
            databaseOptions = databaseOption?.Value ?? throw new ArgumentNullException(nameof(databaseOption));
        }

        public async Task<IUnitOfWork> CreateAsync()
        {
            var conn = new NpgsqlConnection(databaseOptions.ConnectionString);
            await conn.OpenAsync();

            return new UnitOfWork(conn);
        }
    }
}
