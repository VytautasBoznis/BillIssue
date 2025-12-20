using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Auth;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Api.Models.Models.Base;
using Dapper;
using Npgsql;

namespace BillIssue.Api.Business.Repositories.Auth
{
    public class AuthRepository : IAuthRepository
    {
        public async Task<SessionModel?> GetSessionModelByEmail(string email, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@email", email } };

            SessionModel? sessionModel = await unitOfWork
                                                .Connection
                                                .QueryFirstOrDefaultAsync<SessionModel>("SELECT id, password, email, role, is_banned as isBanned, first_name as FirstName, last_name as LastName FROM user_users WHERE email = @email", dictionary);
            return sessionModel;
        }

        public async Task UpdatePassword(Guid userId, string newPasswordHash, string modifiedBy, IUnitOfWork unitOfWork)
        {
            try
            {
                await using NpgsqlCommand updateUserPassword = new NpgsqlCommand("UPDATE user_users SET password = @passwordHash, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE id = @userId", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@userId", userId),
                        new("@passwordHash", newPasswordHash),
                        new("@modifiedBy", modifiedBy),
                        new("@modifiedOn", DateTime.Now),
                    }
                };

                await updateUserPassword.ExecuteNonQueryAsync();
            }
            catch
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task CreateUserAccount(Guid userId, string passwordHash, string email, string firstName, string lastName, IUnitOfWork unitOfWork)
        {
            try
            {
                await using NpgsqlCommand insertUser = new NpgsqlCommand("INSERT INTO user_users (id, email, password, first_name, last_name, created_by) VALUES (@id, @email, @pass, @firstName, @lastName, @createdBy)", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@id", userId),
                        new("@email", email),
                        new("@pass", passwordHash),
                        new("@firstName", firstName),
                        new("@lastName", lastName),
                        new("@createdBy", email),
                    }
                };
                await insertUser.ExecuteNonQueryAsync();
            }
            catch
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
}
    }
}
