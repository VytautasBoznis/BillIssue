using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Shared.Models.Request.Notifications;
using BillIssue.Shared.Models.Response.Notifications;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BillIssue.Api.Business.Notifications
{
    public class CreateWorkspaceNotificationOperation : BaseOperation<CreateWorkspaceNotificationRequest, CreateWorkspaceNotificationResponse>
    {
        public CreateWorkspaceNotificationOperation(
            ILogger<CreateWorkspaceNotificationOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<CreateWorkspaceNotificationRequest> validator) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
        }

        protected override async Task<CreateWorkspaceNotificationResponse> Execute(CreateWorkspaceNotificationRequest request, IUnitOfWork unitOfWork)
        {
            await unitOfWork.BeginTransactionAsync();

            await CreateWorkspaceNotificationInTransaction(request.WorkspaceId, request.TargetUserEmail, request.SessionUserData.Email, unitOfWork);

            return new CreateWorkspaceNotificationResponse();
        }

        private async Task CreateWorkspaceNotificationInTransaction(Guid workspaceId, string targetUserEmail, string userEmail, IUnitOfWork unitOfWork)
        {
            try
            {
                await using NpgsqlCommand insertWorkspaceNotification = new NpgsqlCommand("INSERT INTO workspace_notifications (workspace_id, email, created_by) VALUES (@workspaceId, @email, @createdBy)", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@workspaceId", workspaceId),
                        new("@email", targetUserEmail),
                        new("@createdBy", userEmail),
                    }
                };

                await insertWorkspaceNotification.ExecuteNonQueryAsync();

                _logger.LogInformation($"New workspace invite created for workspace id: {workspaceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    New workspace alert for user invitation failed to create in workspace id {workspaceId} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                await unitOfWork.RollbackAsync();
            }
        }
    }
}
