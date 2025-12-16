using BillIssue.Api.ActionFilters;
using BillIssue.Api.Controllers.Base;
using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Interfaces.Workspace;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Models.Auth;
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

        public WorkspaceController(IWorkspaceFacade WorkspaceFacade, ISessionFacade sessionFacade, ILogger<WorkspaceController> logger) : base(logger)
        {
            _workspaceFacade = WorkspaceFacade;
            _sessionFacade = sessionFacade;
        }

        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        [HttpGet("GetWorkspace/{WorkspaceId}")]
        public async Task<IActionResult> GetWorkspace(Guid WorkspaceId, bool loadUserAssignments)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];

            SessionUserData session = GetSessionModelFromJwt();

            /*string jwtToken = Request.Headers[]

            /*WorkspaceDto result = await _workspaceFacade.GetWorkspace(sessionId, new GetWorkspaceRequest
            {
                WorkspaceId = WorkspaceId,
                LoadWorkspaceUsers = loadUserAssignments
            });

            return Ok(new GetWorkspaceResponse { WorkspaceDto = result });*/

            return Ok();
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

            return Ok(new GetWorkspaceSelectionsForUserResponse { WorkspaceSelections = result });
        }

        [HttpGet("GetAllWorkspacesForUser/{userId}")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> GetAllWorkspacesForUser(Guid userId)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            List<WorkspaceSearchDto> result = await _workspaceFacade.GetAllWorkspacesForUser(sessionId, new GetAllWorkspacesForUserRequest
            {
                UserId = userId
            });

            return Ok(new GetAllWorkspacesForUsersResponse { WorkspaceDtos = result });
        }

        [HttpPost("CreateWorkspace")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> CreateWorkspace([FromBody] CreateWorkspaceRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            WorkspaceDto result = await _workspaceFacade.CreateWorkspace(sessionId, request);

            return Ok(new CreateWorkspaceResponse { WorkspaceDto = result });
        }

        [HttpPatch("UpdateWorkspace")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> UpdateWorkspace([FromBody] UpdateWorkspaceRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            WorkspaceDto result = await _workspaceFacade.UpdateWorkspace(sessionId, request);

            return Ok(new UpdateWorkspaceResponse { WorkspaceDto = result });
        }


        [HttpDelete("RemoveWorkspace/{WorkspaceId}")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> RemoveWorkspace(Guid WorkspaceId)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            await _workspaceFacade.RemoveWorkspace(sessionId, new RemoveWorkspaceRequest
            {
                WorkspaceId = WorkspaceId
            });

            return Ok();
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
