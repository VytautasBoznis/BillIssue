using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Operations.Email;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Auth;
using BillIssue.Api.Interfaces.Repositories.Confirmations;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Request.Auth;
using BillIssue.Shared.Models.Request.Email;
using BillIssue.Shared.Models.Response.Auth;
using BillIssue.Shared.Models.Response.Email;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Auth
{
    public class RemindPasswordOperation : BaseOperation<RemindPasswordRequest, RemindPasswordResponse>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfirmationRepository _confirmationRepository;

        public RemindPasswordOperation(
            ILogger<RemindPasswordOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            IAuthRepository authRepository,
            IConfirmationRepository confirmationRepository,
            OperationFactory operationFactory,
            IValidator<RemindPasswordRequest> validator) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _authRepository = authRepository;
            _confirmationRepository = confirmationRepository;
        }

        protected override async Task<RemindPasswordResponse> Execute(RemindPasswordRequest request, IUnitOfWork unitOfWork)
        {
            SessionModel? sessionModel = await _authRepository.GetSessionModelByEmail(request.Email, unitOfWork);

            if (sessionModel == null || sessionModel.Id == Guid.Empty)
            {
                _logger.LogWarning($"A password remind request was issued for an email that does not exist: {request.Email}");
                return new RemindPasswordResponse();
            }

            await unitOfWork.BeginTransactionAsync();

            Guid passwordReminderGuid = Guid.NewGuid();

            try
            {
                await _confirmationRepository.CreatePasswordReminderConfirmationAsync(passwordReminderGuid, sessionModel.Id, unitOfWork);

                var sendPassowrdReminderRequest = new SendPasswordReminderEmailRequest
                {
                    Email = request.Email,
                    PasswordReminderGuid = passwordReminderGuid
                };

                SendPasswordReminderEmailResponse sendPasswordReminderResponse = await _operationFactory
                                                                                        .Get<SendPasswordReminderEmailOperation>()
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
