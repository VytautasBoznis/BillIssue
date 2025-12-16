using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Request.Auth;
using BillIssue.Shared.Models.Response.Auth;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BillIssue.Api.Business.Auth
{
    public class RemindPasswordConfirmOperation : BaseOperation<RemindPasswordConfirmRequest, RemindPasswordConfirmResponse>
    {
       
        
        public RemindPasswordConfirmOperation(
            ILogger<RemindPasswordConfirmOperation> logger, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            OperationFactory operationFactory, 
            IValidator<RemindPasswordConfirmRequest> validator) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
        }

        protected override async Task<RemindPasswordConfirmResponse> Execute(RemindPasswordConfirmRequest request, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@confirmation_id", request.PasswordReminderGuid } };
            IEnumerable<UserConfirmationModel> userConfirmationModels = await unitOfWork.Connection.QueryAsync<UserConfirmationModel>("SELECT id, user_id, confirmation_type FROM user_confirmations WHERE id = @confirmation_id", dictionary);
            UserConfirmationModel? userConfirmationModel = userConfirmationModels.FirstOrDefault();

            if (userConfirmationModel == null)
            {
                _logger.LogWarning($"An attempt to use an old passoword reminder id {request.PasswordReminderGuid} was made, no user confirmation model found, return a success");
                return new RemindPasswordConfirmResponse();
            }

            await unitOfWork.BeginTransactionAsync();

            try
            {
                await ChangeUserPassword(userConfirmationModel.UserId, request.Password, request.ConfirmPassword, "System", unitOfWork);

                await using NpgsqlCommand deleteUserConfirmation = new NpgsqlCommand("DELETE FROM user_confirmations WHERE id = @userConfirmationId", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@userConfirmationId", request.PasswordReminderGuid)
                    }
                };

                await deleteUserConfirmation.ExecuteNonQueryAsync();

                await unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to change password for password reminder confirmation: {request.PasswordReminderGuid} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);

                await unitOfWork.RollbackAsync();
            }

            return new RemindPasswordConfirmResponse();
        }

        private async Task ChangeUserPassword(Guid userId, string password, string passwordConfirmation, string modifiedBy, IUnitOfWork unitOfWork)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordConfirmation) || password != passwordConfirmation)
            {
                _logger.LogError("Password change failed, because there is an issue with password, password confirmation or they don't match");
                throw new BaseAppException("Failed to update user password", errorCode: ExceptionCodes.AUTH_FAILED_TO_UPDATE_USER);
            }

            if (userId == Guid.Empty)
            {
                _logger.LogError("Password change failed, no user id provided");
                throw new ArgumentNullException(nameof(userId));
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, AuthConstants.PasswordWorkFactor);

            await using NpgsqlCommand updateUserPassword = new NpgsqlCommand("UPDATE user_users SET password = @passwordHash, modified_by = @modifiedBy, modified_on = @modifiedOn WHERE id = @userId", unitOfWork.Connection, unitOfWork.Transaction)
            {
                Parameters =
                {
                    new("@userId", userId),
                    new("@passwordHash", passwordHash),
                    new("@modifiedBy", modifiedBy),
                    new("@modifiedOn", DateTime.Now),
                }
            };

            await updateUserPassword.ExecuteNonQueryAsync();
        }
    }
}
