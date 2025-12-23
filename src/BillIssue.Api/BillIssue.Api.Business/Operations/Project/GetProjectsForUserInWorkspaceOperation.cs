using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Project;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Response.Project;
using BillIssue.Shared.Models.Response.Project.Dto;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Project
{
    public class GetProjectsForUserInWorkspaceOperation : BaseOperation<GetProjectsInWorkspaceForUserRequest, GetProjectsInWorkspaceForUserResponse>
    {
        private readonly IProjectRepository _projectRepository;

        public GetProjectsForUserInWorkspaceOperation(
            ILogger<GetProjectsForUserInWorkspaceOperation> logger, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            OperationFactory operationFactory, 
            IValidator<GetProjectsInWorkspaceForUserRequest> validator,
            IProjectRepository projectRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _projectRepository = projectRepository;
        }

        protected override async Task<GetProjectsInWorkspaceForUserResponse> Execute(GetProjectsInWorkspaceForUserRequest request, IUnitOfWork unitOfWork)
        {
            List<ProjectSearchDto> projectSearchDtos = await _projectRepository.GetAllProjectsForUserInWorkspace(request.SessionUserData.Id, request.WorkspaceId, unitOfWork);

            if (projectSearchDtos == null)
            {
                _logger.LogWarning($"There were no projects found in workspace id: {request.WorkspaceId} for user with id: {request.SessionUserData.Id} .");
            }

            GetProjectsInWorkspaceForUserResponse response = new GetProjectsInWorkspaceForUserResponse
            {
                ProjectSearchDtos = projectSearchDtos ?? new List<ProjectSearchDto>()
            };

            return response;
        }
    }
}
