using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Operations.Project;
using BillIssue.Api.Controllers.Base;
using BillIssue.Api.Models.Constants;
using BillIssue.Shared.Models.Authentication;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Response.Project;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillIssue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : BaseController
    {
        private readonly OperationFactory _operationFactory;
        public ProjectController(OperationFactory operationFactory, ILogger<ProjectController> logger) : base(logger)
        {
            _operationFactory = operationFactory;
        }

        #region Project

        [HttpGet("GetProject/{projectId}")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> GetProject(Guid projectId, bool loadUserAssignments)
        {
            SessionUserData session = GetSessionModelFromJwt();

            GetProjectResponse response = await _operationFactory
                                                    .Get<GetProjectOperation>()
                                                    .Run(new GetProjectRequest
                                                    {
                                                        SessionUserData = session,
                                                        ProjectId = projectId,
                                                        LoadUserAssignments  = loadUserAssignments
                                                    });

            return Ok(response);
        }

        [HttpPost("CreateProject")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            SessionUserData session = GetSessionModelFromJwt();
            request.SessionUserData = session;

            CreateProjectResponse response = await _operationFactory
                                                    .Get<CreateProjectOperation>()
                                                    .Run(request);

            return Ok(response);
        }

        [HttpPatch("ModifyProject")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> ModifyProject([FromBody] ModifyProjectRequest request)
        {
            SessionUserData session = GetSessionModelFromJwt();
            request.SessionUserData = session;

            ModifyProjectResponse response = await _operationFactory
                                                    .Get<ModifyProjectOperation>()
                                                    .Run(request);

            return Ok(response);
        }

        [HttpDelete("RemoveProject/{projectId}")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> RemoveProject(Guid projectId)
        {
            SessionUserData session = GetSessionModelFromJwt();

            RemoveProjectResponse response = await _operationFactory
                                                    .Get<RemoveProjectOperation>()
                                                    .Run(new RemoveProjectRequest
                                                    {
                                                        ProjectId = projectId,
                                                        SessionUserData = session
                                                    });

            return Ok(response);
        }

        [HttpGet("GetProjectsForUserInWorkspace/{workspaceId}")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> GetProjectsForUserInWorkspace(Guid workspaceId)
        {
            SessionUserData session = GetSessionModelFromJwt();

            GetProjectsInWorkspaceForUserResponse response = await _operationFactory
                                                                    .Get<GetProjectsForUserInWorkspaceOperation>()
                                                                    .Run(new GetProjectsInWorkspaceForUserRequest
                                                                    {
                                                                        WorkspaceId = workspaceId,
                                                                        SessionUserData = session
                                                                    });

            return Ok(response);
        }

        #endregion

        #region Project worktype

        [HttpGet("GetProjectWorktype/{projectWorktypeId}")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> GetProjectWorktype(Guid projectWorktypeId)
        {
            SessionUserData session = GetSessionModelFromJwt();

            GetProjectWorktypeResponse response = await _operationFactory
                                                            .Get<GetProjectWorktypeOperation>()
                                                            .Run(new GetProjectWorktypeRequest
                                                            {
                                                                ProjectWorktypeId = projectWorktypeId,
                                                                SessionUserData = session
                                                            });

            return Ok(response);
        }

        [HttpGet("GetAllProjectWorktypes/{projectId}")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> GetAllProjectWorktypes(Guid projectId)
        {
            SessionUserData session = GetSessionModelFromJwt();

            GetAllProjectWorktypesResponse response = await _operationFactory
                                                                .Get<GetAllProjectWorktypesOperation>()
                                                                .Run(new GetAllProjectWorktypesRequest
                                                                {
                                                                    ProjectId = projectId,
                                                                    SessionUserData = session
                                                                });

            return Ok(response);
        }

        [HttpPost("CreateProjectWorktype")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> CreateProjectWorktype([FromBody] CreateProjectWorktypeRequest request)
        {
            SessionUserData session = GetSessionModelFromJwt();
            request.SessionUserData = session;

            CreateProjectWorktypeResponse response = await _operationFactory
                                                                .Get<CreateProjectWorktypeOperation>()
                                                                .Run(request);

            return Ok(response);
        }

        [HttpPatch("ModifyProjectWorktype")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> ModifyProjectWorktype([FromBody] ModifyProjectWorktypeRequest request)
        {
            SessionUserData session = GetSessionModelFromJwt();
            request.SessionUserData = session;

            ModifyProjectWorktypeResponse response = await _operationFactory
                                                                .Get<ModifyProjectWorktypeOperation>()
                                                                .Run(request);

            return Ok(response);
        }

        [HttpDelete("RemoveProjectWorktype/{projectId}/{projectWorktypeId}")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> RemoveProjectWorktype(Guid projectId, Guid projectWorktypeId)
        {
            SessionUserData session = GetSessionModelFromJwt();

            RemoveProjectWorktypeResponse response = await _operationFactory
                                                                .Get<RemoveProjectWorktypeOperation>()
                                                                .Run(new RemoveProjectWorktypeRequest
                                                                {
                                                                    ProjectId = projectId,
                                                                    ProjectWorktypeId = projectWorktypeId,
                                                                    SessionUserData = session
                                                                });

            return Ok(session);
        }

        #endregion

        #region Project User Assignment

        [HttpGet("GetProjectUsers/{projectId}")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> GetProjectUsers(Guid projectId)
        {
            SessionUserData session = GetSessionModelFromJwt();

            GetProjectUsersResponse response = await _operationFactory
                                                        .Get<GetProjectUsersOperation>()
                                                        .Run(new GetProjectUsersRequest
                                                        {
                                                            ProjectId = projectId,
                                                            SessionUserData = session
                                                        });

            return Ok(response);
        }

        [HttpGet("GetProjectsForUser")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> GetProjectsForUser()
        {
            SessionUserData session = GetSessionModelFromJwt();

            GetProjectsForUserResponse response = await _operationFactory
                                                            .Get<GetProjectsForUserOperation>()
                                                            .Run(new GetProjectsForUserRequest
                                                            {
                                                                SessionUserData = session
                                                            });

            return Ok(response);
        }

        [HttpPost("AddUserAssignmentToProject")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> AddUserAssignmentToProject([FromBody] AddUserAssignmentToProjectRequest request)
        {
            SessionUserData session = GetSessionModelFromJwt();
            request.SessionUserData = session;

            AddUserAssignmentToProjectResponse response = await _operationFactory
                                                                    .Get<AddUserAssignmentToProjectOperation>()
                                                                    .Run(request);

            return Ok(response);
        }

        [HttpPatch("ModifyUserAssingmentInProject")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> ModifyUserAssingmentInProject([FromBody] ModifyUserAssingmentInProjectRequest request)
        {
            SessionUserData session = GetSessionModelFromJwt();
            request.SessionUserData = session;

            ModifyUserAssingmentInProjectResponse response = await _operationFactory
                                                                    .Get<ModifyUserAssingmentInProjectOperation>()
                                                                    .Run(request);

            return Ok(response);
        }

        [HttpDelete("RemoveUserAssingmentFromProject/{projectId}/{projectUserAssignmentId}")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> RemoveUserAssingmentFromProject(Guid projectId, Guid projectUserAssignmentId)
        {
            SessionUserData session = GetSessionModelFromJwt();

            RemoveUserAssingmentFromProjectResponse response = await _operationFactory
                                                                        .Get<RemoveUserAssingmentFromProjectOperation>()
                                                                        .Run(new RemoveUserAssingmentFromProjectRequest
                                                                        {
                                                                            ProjectId = projectId,
                                                                            ProjectUserAssignmentId = projectUserAssignmentId,
                                                                            SessionUserData = session
                                                                        });

            return Ok(response);
        }

        #endregion
    }
}
