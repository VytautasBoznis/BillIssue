using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Response.Workspace;
using BillIssue.Shared.Models.Response.Workspace.Dto;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Workspace
{
    public class GetWorkspaceOperation : BaseOperation<GetWorkspaceRequest, GetWorkspaceResponse>
    {
        public GetWorkspaceOperation(
            ILogger<GetWorkspaceOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<GetWorkspaceRequest> validator) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
        }

        protected override async Task<GetWorkspaceResponse> Execute(GetWorkspaceRequest request, IUnitOfWork unitOfWork)
        {
            WorkspaceDto workspaceDto = await GetWorkspaceDataWithPermissionCheck(request.SessionUserData.Id, request.WorkspaceId, WorkspaceUserRole.Manager, unitOfWork, request.SessionUserData.Role == UserRole.Admin);

            if (workspaceDto == null)
            {
                _logger.LogError($"User with user id: {request.SessionUserData.Id} tried to access an unknow workspace with id: {request.WorkspaceId}.");
                throw new WorkspaceException("Workspace not found", ExceptionCodes.WORKSPACE_NOT_FOUND);
            }

            if (request.LoadWorkspaceUsers)
            {
                workspaceDto.WorkspaceUsers = await GetAllWorkspaceUsers(workspaceDto.Id, unitOfWork);
            }

            return new GetWorkspaceResponse
            {
                WorkspaceDto = workspaceDto
            };
        }

        private async Task<WorkspaceDto> GetWorkspaceDataWithPermissionCheck(Guid userId, Guid WorkspaceId, WorkspaceUserRole minimalRole, IUnitOfWork unitOfWork, bool isAdmin = false)
        {
            var dictionary = new Dictionary<string, object> { { "@wi", WorkspaceId } };

            IEnumerable<WorkspaceDto> workspaceDtos = await unitOfWork.Connection.QueryAsync<WorkspaceDto>("SELECT ww.id, ww.name, ww.description, ww.is_deleted as IsDeleted FROM workspace_workspaces ww WHERE id = @wi", dictionary);
            WorkspaceDto? workspaceDto = workspaceDtos.FirstOrDefault();

            if (workspaceDto == null)
            {
                _logger.LogError($"User with user id: {userId} tried to get an unknow company workspace with id: {WorkspaceId}.");
                throw new ProjectException("Company workspace not found", ExceptionCodes.WORKSPACE_NOT_FOUND);
            }

            if (workspaceDto.IsDeleted)
            {
                _logger.LogError($"User with user id: {userId} tried to get an company workspace that was deleted with id: {WorkspaceId}.");
                throw new ProjectException("Company workspace not found", ExceptionCodes.WORKSPACE_NOT_FOUND);
            }

            return workspaceDto;
        }
        public async Task<List<WorkspaceUserDto>> GetAllWorkspaceUsers(Guid wokspaceId, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@wi", wokspaceId } };
            IEnumerable<WorkspaceUserDto> workspaceUsers = await unitOfWork.Connection.QueryAsync<WorkspaceUserDto>(@"
                SELECT WUA.id as assignmentId, UU.id as userId, UU.email, UU.first_name, UU.last_name, WUA.workspace_role as role
                FROM workspace_workspaces ww
                    JOIN workspace_user_assignments WUA
                        ON ww.id = WUA.workspace_id
                    JOIN user_users uu
                        ON WUA.user_id = UU.id
                WHERE ww.id=@wi
                    AND ww.is_deleted = false", dictionary);

            return workspaceUsers.ToList();
        }
    }
}
