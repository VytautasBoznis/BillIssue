using BillIssue.Api.ActionFilters;
using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Operations.Workspace;
using BillIssue.Api.Controllers.Base;
using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Interfaces.Workspace;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Shared.Models.Authentication;
using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Response.Workspace;
using BillIssue.Shared.Models.Response.Workspace.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillIssue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkspaceController : BaseController
    {
        private readonly IWorkspaceFacade _workspaceFacade;
        private readonly ISessionFacade _sessionFacade;
        private readonly OperationFactory _operationFactory;

        public WorkspaceController(IWorkspaceFacade WorkspaceFacade, ISessionFacade sessionFacade, ILogger<WorkspaceController> logger, OperationFactory operationFactory) : base(logger)
        {
            _workspaceFacade = WorkspaceFacade;
            _sessionFacade = sessionFacade;
            _operationFactory = operationFactory;
        }

        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        [HttpGet("GetWorkspace/{WorkspaceId}")]
        public async Task<IActionResult> GetWorkspace(Guid WorkspaceId, bool loadUserAssignments)
        {
            SessionUserData session = GetSessionModelFromJwt();

            GetWorkspaceResponse response = await _operationFactory
                                                .Get<GetWorkspaceOperation>(typeof(GetWorkspaceOperation))
                                                .Run(new GetWorkspaceRequest
                                                {
                                                    SessionUserData = session,
                                                    WorkspaceId = WorkspaceId,
                                                    LoadWorkspaceUsers = loadUserAssignments
                                                });

            return Ok(response);
        }

        [HttpGet("GetAllWorkspaceSelectionsForUser/{userId}")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> GetAllWorkspaceSelectionsForUser(Guid userId)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            List<WorkspaceSelectionDto> result = await _workspaceFacade.GetAllWorkspaceSelectionsForUser(sessionId, new GetWorkspaceSelectionsForUserRequest
            {
                UserId = userId
            });

            SessionUserData session = GetSessionModelFromJwt();

            GetWorkspaceSelectionsForUserResponse response = await _operationFactory
                                                                        .Get<GetWorkspaceSelectionsForUserOperation>(typeof(GetWorkspaceSelectionsForUserOperation))
                                                                        .Run(new GetWorkspaceSelectionsForUserRequest
                                                                        {
                                                                            SessionUserData = session,
                                                                            UserId = userId
                                                                        });

            return Ok(response);
        }

        [HttpGet("GetAllWorkspacesForUser/{userId}")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> GetAllWorkspacesForUser(Guid userId)
        {
            SessionUserData session = GetSessionModelFromJwt();

            GetAllWorkspacesForUsersResponse response = await _operationFactory
                                                .Get<GetAllWorkspacesForUserOperation>(typeof(GetAllWorkspacesForUserOperation))
                                                .Run(new GetAllWorkspacesForUserRequest
                                                {
                                                    SessionUserData = session,
                                                    UserId = userId
                                                });

            return Ok(response);
        }

        [HttpPost("CreateWorkspace")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> CreateWorkspace([FromBody] CreateWorkspaceRequest request)
        {
            SessionUserData session = GetSessionModelFromJwt();
            request.SessionUserData = session;

            CreateWorkspaceResponse response = await _operationFactory
                                                .Get<CreateWorkspaceOperation>(typeof(CreateWorkspaceOperation))
                                                .Run(request);

            return Ok(response);
        }

        [HttpPatch("UpdateWorkspace")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> UpdateWorkspace([FromBody] ModifyWorkspaceRequest request)
        {
            SessionUserData session = GetSessionModelFromJwt();
            request.SessionUserData = session;

            ModifyWorkspaceResponse response = await _operationFactory
                                                        .Get<ModifyWorkspaceOperation>(typeof(ModifyWorkspaceOperation))
                                                        .Run(request);

            return Ok(response);
        }


        [HttpDelete("RemoveWorkspace/{WorkspaceId}")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> RemoveWorkspace(Guid WorkspaceId)
        {
            SessionUserData session = GetSessionModelFromJwt();

            RemoveWorkspaceResponse response = await _operationFactory
                                                        .Get<RemoveWorkspaceOperation>(typeof(RemoveWorkspaceOperation))
                                                        .Run(new RemoveWorkspaceRequest
                                                        {
                                                            SessionUserData = session,
                                                            WorkspaceId = WorkspaceId
                                                        });

            return Ok(response);
        }

        [HttpGet("GetAllWorkspaceUsers/{WorkspaceId}")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> GetAllWorkspaceUsers(Guid WorkspaceId)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            List<WorkspaceUserDto> result = await _workspaceFacade.GetAllWorkspaceUsers(sessionId, new GetAllWorkspaceUsersRequest
            {
                WorkspaceId = WorkspaceId
            });

            return Ok(new GetAllWorkspaceUsersResponse { WorkspaceUserDtos = result });
        }

        [HttpPost("AddUserToWorkspace")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> AddUserToWorkspace([FromBody] AddUserToWorkspaceRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            await _workspaceFacade.AddUserToWorkspace(sessionId, request);

            return Ok();
        }

        [HttpPatch("UpdateUserInWorkspace")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> UpdateUserInWorkspace([FromBody] UpdateUserInWorkspaceRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            await _workspaceFacade.UpdateUserInWorkspace(sessionId, request);

            return Ok();
        }

        [HttpDelete("RemoveUserFromWorkspace")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> RemoveUserFromWorkspace([FromBody] RemoveUserFromWorkspaceRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            await _workspaceFacade.RemoveUserFromWorkspace(sessionId, request);

            return Ok();
        }
    }
}
