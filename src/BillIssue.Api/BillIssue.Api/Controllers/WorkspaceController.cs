using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Operations.Workspace;
using BillIssue.Api.Controllers.Base;
using BillIssue.Api.Models.Constants;
using BillIssue.Shared.Models.Authentication;
using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Response.Workspace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillIssue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkspaceController : BaseController
    {
        private readonly OperationFactory _operationFactory;

        public WorkspaceController(ILogger<WorkspaceController> logger, OperationFactory operationFactory) : base(logger)
        {
            _operationFactory = operationFactory;
        }

        [HttpGet("GetWorkspace/{WorkspaceId}")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
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
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> GetAllWorkspaceSelectionsForUser(Guid userId)
        {
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
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
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
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
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
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
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
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
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
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> GetAllWorkspaceUsers(Guid WorkspaceId)
        {
            SessionUserData session = GetSessionModelFromJwt();

            GetAllWorkspaceUsersResponse response = await _operationFactory
                                                            .Get<GetAllWorkspaceUsersOperation>(typeof(GetAllWorkspaceUsersOperation))
                                                            .Run(new GetAllWorkspaceUsersRequest
                                                            {
                                                                SessionUserData = session,
                                                                WorkspaceId = WorkspaceId
                                                            });

            return Ok(response);
        }

        [HttpPost("AddUserToWorkspace")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> AddUserToWorkspace([FromBody] AddUserToWorkspaceRequest request)
        {
            SessionUserData session = GetSessionModelFromJwt();
            request.SessionUserData = session;

            AddUserToWorkspaceResponse response = await _operationFactory
                                                            .Get<AddUserToWorkspaceOperation>(typeof(AddUserToWorkspaceOperation))
                                                            .Run(request);

            return Ok(response);
        }

        [HttpPatch("UpdateUserInWorkspace")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> UpdateUserInWorkspace([FromBody] ModifyUserInWorkspaceRequest request)
        {
            SessionUserData session = GetSessionModelFromJwt();
            request.SessionUserData = session;

            ModifyUserInWorkspaceResponse response = await _operationFactory
                                                            .Get<ModifyUserInWorkspaceOperation>(typeof(ModifyUserInWorkspaceOperation))
                                                            .Run(request);

            return Ok(response);
        }

        [HttpDelete("RemoveUserFromWorkspace")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> RemoveUserFromWorkspace([FromBody] RemoveUserFromWorkspaceRequest request)
        {
            SessionUserData session = GetSessionModelFromJwt();
            request.SessionUserData = session;

            RemoveUserFromWorkspaceResponse response = await _operationFactory
                                                                .Get<RemoveUserFromWorkspaceOperation>(typeof(RemoveUserFromWorkspaceOperation))
                                                                .Run(request);

            return Ok(response);
        }
    }
}
