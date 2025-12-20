using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Operations.Workspace;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Api.Interfaces.Repositories.Auth;
using BillIssue.Shared.Models.Authentication;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Request.Auth;
using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Response.Auth;
using BillIssue.Shared.Models.Response.Workspace;
using FluentValidation;
using Microsoft.Extensions.Logging;
using RegisterRequest = BillIssue.Shared.Models.Request.Auth.RegisterRequest;

namespace BillIssue.Api.Business.Operations.Auth
{
    public class RegisterOperation : BaseOperation<RegisterRequest, RegisterResponse>
    {
        private readonly IAuthRepository _authRepository;
        private const string NewUsersWorkspaceTagline = "{0}'s personal workspace";

        public RegisterOperation(
            ILogger<RegisterOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IAuthRepository authRepository,
            IValidator<RegisterRequest> validator) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _authRepository = authRepository;
        }

        protected override async Task<RegisterResponse> Execute(RegisterRequest request, IUnitOfWork unitOfWork)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, AuthConstants.PasswordWorkFactor);

            try
            {
                SessionModel? sessionModel = await _authRepository.GetSessionModelByEmail(request.Email, unitOfWork);

                if (sessionModel != null)
                {
                    throw new RegistrationException("Email already in use", ExceptionCodes.AUTH_REGISTRATION_EMAIL_ALREADY_IN_USE);
                }

                Guid newUserId = Guid.NewGuid();

                await unitOfWork.BeginTransactionAsync();

                await _authRepository.CreateUserAccount(newUserId, passwordHash, request.Email, request.FirstName, request.LastName, unitOfWork);

                CreateWorkspaceRequest createWorksapceRequest = new CreateWorkspaceRequest
                {
                    SessionUserData = new SessionUserData
                    {
                        Id = newUserId,
                        Email = request.Email,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                    },
                    Name = string.Format(NewUsersWorkspaceTagline, request.FirstName),
                    Description = string.Format(NewUsersWorkspaceTagline, request.FirstName),
                };

                CreateWorkspaceResponse createWorkspaceResponse = await _operationFactory
                                                                            .Get<CreateWorkspaceOperation>(typeof(CreateWorkspaceOperation))
                                                                            .Run(createWorksapceRequest, unitOfWork);
                
                await unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();

                if (ex is RegistrationException)
                {
                    throw;
                }

                _logger.LogError($"A not standart exception happened in registration: {ex.Message} \n\n Stacktrace: {ex.StackTrace}");
                throw new BaseAppException("Error occured during registration!", errorCode: ExceptionCodes.UNEXPECTED_EXCEPTION);
            }


            LoginRequest loginRequest = new LoginRequest
            {
                Email = request.Email,
                Password = request.Password,
            };

            LoginResponse loginResponse = await _operationFactory
                                                    .Get<LoginOperation>(typeof(LoginOperation))
                                                    .Run(loginRequest, unitOfWork);

            return new RegisterResponse()
            {
                Session = loginResponse.Session
            };
        }
    }
}
