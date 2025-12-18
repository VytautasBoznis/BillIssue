using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Confirmations;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Models.Auth;
using Dapper;
using Npgsql;

namespace BillIssue.Api.Business.Repositories.Confirmations
{
    public class ConfirmationRepository : IConfirmationRepository
    {
        public async Task<UserConfirmationModel?> GetConfirmationById(Guid confirmationId, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@confirmation_id", confirmationId } };
            UserConfirmationModel? userConfirmationModels = await unitOfWork.Connection.QueryFirstOrDefaultAsync<UserConfirmationModel>("SELECT id, user_id, confirmation_type FROM user_confirmations WHERE id = @confirmation_id", dictionary);

            return userConfirmationModels;
        }

        public async Task CreatePasswordReminderConfirmationAsync(Guid confirmationId, Guid userId, IUnitOfWork unitOfWork)
        {
            await using NpgsqlCommand insertPasswordReminder = new NpgsqlCommand("INSERT INTO user_confirmations (id, user_id, confirmation_type, created_by) VALUES (@id, @userId, @confirmationType, @createdBy)", unitOfWork.Connection, unitOfWork.Transaction)
            {
                Parameters =
                    {
                        new("@id", confirmationId),
                        new("@userId", userId),
                        new("@confirmationType", (int) ConfirmationTypeEnum.RemindPassword),
                        new("@createdBy", "System password reminder"),
                    }
            };

            await insertPasswordReminder.ExecuteNonQueryAsync();
        }

        public async Task DeleteConfirmationAsync(Guid confirmationId, IUnitOfWork unitOfWork)
        {
            await using NpgsqlCommand deleteUserConfirmation = new NpgsqlCommand("DELETE FROM user_confirmations WHERE id = @userConfirmationId", unitOfWork.Connection, unitOfWork.Transaction)
            {
                Parameters =
                {
                    new("@userConfirmationId", confirmationId)
                }
            };

            await deleteUserConfirmation.ExecuteNonQueryAsync();
        }
    }
}
