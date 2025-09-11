using BillIssue.Api.ActionFilters;
using BillIssue.Api.Controllers.Base;
using BillIssue.Api.Interfaces.Project;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Enums.Auth;
using Microsoft.AspNetCore.Mvc;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Response.Project.Dto;
using BillIssue.Shared.Models.Response.Project;

namespace BillIssue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : BaseController
    {
        private readonly IProjectFacade _projectFacade;
        public ProjectController(IProjectFacade projectFacade)
        {
            _projectFacade = projectFacade;
        }

        #region Project

        [HttpGet("GetProject/{projectId}")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> GetProject(Guid projectId, bool loadUserAssignments)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            ProjectDto result = await _projectFacade.GetProject(sessionId, new GetProjectRequest { ProjectId = projectId, LoadUserAssignments = loadUserAssignments });

            return Ok(new GetProjectResponse { ProjectDto = result});
        }

        [HttpPost("CreateProject")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            ProjectDto result = await _projectFacade.CreateProject(sessionId, request);

            return Ok(new CreateProjectResponse { ProjectDto = result });
        }

        [HttpPatch("ModifyProject")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> ModifyProject([FromBody] ModifyProjectRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            await _projectFacade.ModifyProject(sessionId, request);

            return Ok();
        }

        [HttpDelete("RemoveProject/{projectId}")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> RemoveProject(Guid projectId)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            await _projectFacade.RemoveProject(sessionId, new RemoveProjectRequest
            {
                ProjectId = projectId
            });

            return Ok();
        }

        [HttpGet("GetProjectsForUserInWorkspace/{workspaceId}")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> GetProjectsForUserInWorkspace(Guid workspaceId)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            List<ProjectSearchDto> result = await _projectFacade.GetUserProjectsInWorkspace(sessionId, new GetUserProjectsInWorkspaceForUserRequest { WorkspaceId = workspaceId });

            return Ok(new GetUserProjectsInWorkspaceForUserResponse { ProjectSearchDtos = result });
        }

        #endregion

        #region Project worktype

        [HttpGet("GetProjectWorktype/{projectWorktypeId}")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> GetProjectWorktype(Guid projectWorktypeId)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            ProjectWorktypeDto result = await _projectFacade.GetProjectWorktype(sessionId, new GetProjectWorktypeRequest { ProjectWorktypeId = projectWorktypeId });

            return Ok(new GetProjectWorktypeResponse { ProjectWorktypeDto = result });
        }

        [HttpGet("GetAllProjectWorktypes/{projectId}")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> GetAllProjectWorktypes(Guid projectId)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            List<ProjectWorktypeDto> result = await _projectFacade.GetAllProjectWorktypes(sessionId, new GetAllProjectWorktypesRequest { ProjectId = projectId });

            return Ok(new GetAllProjectWorktypesResponse { ProjectWorktypeDtos = result });
        }

        [HttpPost("CreateProjectWorktype")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> CreateProjectWorktype([FromBody] CreateProjectWorktypeRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            await _projectFacade.CreateProjectWorktype(sessionId, request);

            return Ok();
        }

        [HttpPatch("ModifyProjectWorktype")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> ModifyProjectWorktype([FromBody] ModifyProjectWorktypeRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            await _projectFacade.ModifyProjectWorktype(sessionId, request);

            return Ok();
        }

        [HttpDelete("RemoveProjectWorktype/{projectId}/{projectWorktypeId}")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> RemoveProjectWorktype(Guid projectId, Guid projectWorktypeId)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            await _projectFacade.RemoveProjectWorktype(sessionId, new RemoveProjectWorktypeRequest
            {
                ProjectId = projectId,
                ProjectWorktypeId = projectWorktypeId
            });

            return Ok();
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
