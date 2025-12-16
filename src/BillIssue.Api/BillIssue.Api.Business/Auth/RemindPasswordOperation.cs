using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Email;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Request.Auth;
using BillIssue.Shared.Models.Request.Email;
using BillIssue.Shared.Models.Response.Auth;
using BillIssue.Shared.Models.Response.Email;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BillIssue.Api.Business.Auth
{
    public class RemindPasswordOperation : BaseOperation<RemindPasswordRequest, RemindPasswordResponse>
    {
        public RemindPasswordOperation(
            ILogger<RemindPasswordOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<RemindPasswordRequest> validator) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
        }

        protected override async Task<RemindPasswordResponse> Execute(RemindPasswordRequest request, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@email", request.Email } };
            IEnumerable<SessionModel> sessionModels = await unitOfWork.Connection.QueryAsync<SessionModel>("SELECT id, email FROM user_users WHERE LOWER(email)=LOWER(@email)", dictionary);
            SessionModel? sessionModel = sessionModels.FirstOrDefault();

            if (sessionModel == null || sessionModel.Id == Guid.Empty)
            {
                _logger.LogWarning($"A password remind request was issued for an email that does not exist: {request.Email}");
                return new RemindPasswordResponse();
            }

            await unitOfWork.BeginTransactionAsync();

            Guid passwordReminderGuid = Guid.NewGuid();

            try
            {
                await using NpgsqlCommand insertPasswordReminder = new NpgsqlCommand("INSERT INTO user_confirmations (id, user_id, confirmation_type, created_by) VALUES (@id, @userId, @confirmationType, @createdBy)", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@id", passwordReminderGuid),
                        new("@userId", sessionModel.Id),
                        new("@confirmationType", (int) ConfirmationTypeEnum.RemindPassword),
                        new("@createdBy", "System password reminder"),
                    }
                };

                await insertPasswordReminder.ExecuteNonQueryAsync();

                var sendPassowrdReminderRequest = new SendPasswordReminderEmailRequest
                {
                    Email = request.Email,
                    PasswordReminderGuid = passwordReminderGuid
                };

                SendPasswordReminderEmailResponse sendPasswordReminderResponse = await _operationFactory
                                                                                        .Get<SendPasswordReminderEmailOperation>(typeof(SendPasswordReminderEmailOperation))
                                                                                        .Run(sendPassowrdReminderRequest, unitOfWork);

                await unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to create and send password reminder for email: {request.Email} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                await unitOfWork.RollbackAsync();
            }

            return new RemindPasswordResponse();
        }
    }
}
