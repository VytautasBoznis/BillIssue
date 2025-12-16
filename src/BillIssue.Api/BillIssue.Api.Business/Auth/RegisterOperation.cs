using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Workspace;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Authentication;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Request.Auth;
using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Response.Auth;
using BillIssue.Shared.Models.Response.Workspace;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Npgsql;
using RegisterRequest = BillIssue.Shared.Models.Request.Auth.RegisterRequest;

namespace BillIssue.Api.Business.Auth
{
    public class RegisterOperation : BaseOperation<RegisterRequest, RegisterResponse>
    {
        private const int PasswordWorkFactor = 12;
        private const string NewUsersWorkspaceTagline = "{0}'s personal workspace";

        public RegisterOperation(
            ILogger<RegisterOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<RegisterRequest> validator) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
        }

        protected override async Task<RegisterResponse> Execute(RegisterRequest request, IUnitOfWork unitOfWork)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, PasswordWorkFactor);

            try
            {
                var dictionary = new Dictionary<string, object> { { "@email", request.Email } };

                IEnumerable<SessionModel> sessionModels = await unitOfWork.Connection.QueryAsync<SessionModel>("SELECT email FROM user_users WHERE LOWER(email)=LOWER(@email)", dictionary);
                SessionModel? sessionModel = sessionModels.FirstOrDefault();

                if (sessionModel != null)
                {
                    throw new RegistrationException("Email already in use", ExceptionCodes.AUTH_REGISTRATION_EMAIL_ALREADY_IN_USE);
                }

                Guid newUserId = Guid.NewGuid();

                await unitOfWork.BeginTransactionAsync();

                await using NpgsqlCommand insertUser = new NpgsqlCommand("INSERT INTO user_users (id, email, password, first_name, last_name, created_by) VALUES (@id, @email, @pass, @firstName, @lastName, @createdBy)", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@id", newUserId),
                        new("@email", request.Email),
                        new("@pass", passwordHash),
                        new("@firstName", request.FirstName),
                        new("@lastName", request.LastName),
                        new("@createdBy", request.Email),
                    }
                };

                await insertUser.ExecuteNonQueryAsync();

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
