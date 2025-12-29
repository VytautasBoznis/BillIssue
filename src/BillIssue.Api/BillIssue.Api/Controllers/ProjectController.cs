using BillIssue.Api.ActionFilters;
using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Operations.Project;
using BillIssue.Api.Controllers.Base;
using BillIssue.Api.Interfaces.Project;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Shared.Models.Authentication;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Response.Project;
using BillIssue.Shared.Models.Response.Project.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillIssue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : BaseController
    {
        private readonly IProjectFacade _projectFacade;
        private readonly OperationFactory _operationFactory;
        public ProjectController(IProjectFacade projectFacade, OperationFactory operationFactory, ILogger<ProjectController> logger) : base(logger)
        {
            _projectFacade = projectFacade;
            _operationFactory = operationFactory;
        }

        #region Project

        [HttpGet("GetProject/{projectId}")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> GetProject(Guid projectId, bool loadUserAssignments)
        {
            SessionUserData session = GetSessionModelFromJwt();

            GetProjectResponse response = await _operationFactory
                                                    .Get<GetProjectOperation>(typeof(GetProjectOperation))
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
                                                    .Get<CreateProjectOperation>(typeof(CreateProjectOperation))
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
                                                    .Get<ModifyProjectOperation>(typeof(ModifyProjectOperation))
                                                    .Run(request);

            return Ok(response);
        }

        [HttpDelete("RemoveProject/{projectId}")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> RemoveProject(Guid projectId)
        {
            SessionUserData session = GetSessionModelFromJwt();

            RemoveProjectResponse response = await _operationFactory
                                                    .Get<RemoveProjectOperation>(typeof(RemoveProjectOperation))
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
                                                                    .Get<GetProjectsForUserInWorkspaceOperation>(typeof(GetProjectsForUserInWorkspaceOperation))
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
                                                            .Get<GetProjectWorktypeOperation>(typeof(GetProjectWorktypeOperation))
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
                                                                .Get<GetAllProjectWorktypesOperation>(typeof(GetAllProjectWorktypesOperation))
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
                                                                .Get<CreateProjectWorktypeOperation>(typeof(CreateProjectWorktypeOperation))
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
                                                                .Get<ModifyProjectWorktypeOperation>(typeof(ModifyProjectWorktypeOperation))
                                                                .Run(request);

            return Ok(response);
        }

        [HttpDelete("RemoveProjectWorktype/{projectId}/{projectWorktypeId}")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> RemoveProjectWorktype(Guid projectId, Guid projectWorktypeId)
        {
            SessionUserData session = GetSessionModelFromJwt();

            RemoveProjectWorktypeResponse response = await _operationFactory
                                                                .Get<RemoveProjectWorktypeOperation>(typeof(RemoveProjectWorktypeOperation))
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
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> GetProjectUsers(Guid projectId)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            List<ProjectUserAssignmentDto> result = await _projectFacade.GetProjectUsers(sessionId, new GetProjectUsersRequest { ProjectId = projectId });

            return Ok(new GetProjectUsersResponse { ProjectUserAssignmentDtos = result });
        }

        [HttpGet("GetProjectsForUser")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> GetProjectsForUser()
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            List<ProjectDto> result = await _projectFacade.GetProjectsForUser(sessionId);

            return Ok(new GetProjectsForUserResponse { ProjectDtos = result});
        }

        [HttpPost("AddUserAssignmentToProject")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> AddUserAssignmentToProject([FromBody] AddUserAssignmentToProjectRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            await _projectFacade.AddUserAssignmentToProject(sessionId, request);

            return Ok();
        }

        [HttpPatch("ModifyUserAssingmentInProject")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> ModifyUserAssingmentInProject([FromBody] ModifyUserAssingmentInProjectRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            await _projectFacade.ModifyUserAssingmentInProject(sessionId, request);

            return Ok();
        }

        [HttpDelete("RemoveUserAssingmentFromProject/{projectId}/{projectUserAssignmentId}")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> RemoveUserAssingmentFromProject(Guid projectId, Guid projectUserAssignmentId)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            await _projectFacade.RemoveUserAssingmentFromProject(sessionId, new RemoveUserAssingmentFromProjectRequest
            {
                ProjectId = projectId,
                ProjectUserAssignmentId = projectUserAssignmentId
            });

            return Ok();
        }

        #endregion
    }
}
