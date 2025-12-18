using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Auth;
using BillIssue.Api.Interfaces.Repositories.Confirmations;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Request.Auth;
using BillIssue.Shared.Models.Response.Auth;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Auth
{
    public class RemindPasswordConfirmOperation : BaseOperation<RemindPasswordConfirmRequest, RemindPasswordConfirmResponse>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfirmationRepository _confirmationRepository;

        public RemindPasswordConfirmOperation(
            ILogger<RemindPasswordConfirmOperation> logger, 
            IUnitOfWorkFactory unitOfWorkFactory,
            IAuthRepository authRepository,
            IConfirmationRepository confirmationRepository,
            OperationFactory operationFactory, 
            IValidator<RemindPasswordConfirmRequest> validator) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _authRepository = authRepository;
            _confirmationRepository = confirmationRepository;
        }

        protected override async Task<RemindPasswordConfirmResponse> Execute(RemindPasswordConfirmRequest request, IUnitOfWork unitOfWork)
        {
            UserConfirmationModel? userConfirmationModel = await _confirmationRepository.GetConfirmationById(request.PasswordReminderGuid, unitOfWork);

            if (userConfirmationModel == null)
            {
                _logger.LogWarning($"An attempt to use an old passoword reminder id {request.PasswordReminderGuid} was made, no user confirmation model found, return a success");
                return new RemindPasswordConfirmResponse();
            }

            await unitOfWork.BeginTransactionAsync();

            try
            {
                await ChangeUserPassword(userConfirmationModel.UserId, request.Password, request.ConfirmPassword, "System", unitOfWork);
                await _confirmationRepository.DeleteConfirmationAsync(request.PasswordReminderGuid,  unitOfWork);
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

            await _authRepository.UpdatePassword(userId, passwordHash, modifiedBy, unitOfWork);
        }
    }
}
