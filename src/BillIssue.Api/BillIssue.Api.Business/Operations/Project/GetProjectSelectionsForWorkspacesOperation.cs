using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Project;
using BillIssue.Api.Models.Models.Projects;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Response.Project;
using BillIssue.Shared.Models.Response.Project.Dto;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Project
{
    public class GetProjectSelectionsForWorkspacesOperation : BaseOperation<GetProjectSelectionsForWorkspacesRequest, GetProjectSelectionsForWorkspacesResponse>
    {
        private readonly IProjectRepository _projectRepository;

        public GetProjectSelectionsForWorkspacesOperation(
            ILogger<GetProjectSelectionsForWorkspacesOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<GetProjectSelectionsForWorkspacesRequest> validator,
            IProjectRepository projectRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _projectRepository = projectRepository;
        }

        protected override async Task<GetProjectSelectionsForWorkspacesResponse> Execute(GetProjectSelectionsForWorkspacesRequest request, IUnitOfWork unitOfWork)
        {
            List<ProjectAndWorktypeModelFlattened> projectAndWorktypeModelsFlattened = await _projectRepository.GetAllProjectSelectionsForWorkspacesFlattened(request.WorkspaceIds, unitOfWork);
            List<Guid> uniqueProjectIds = projectAndWorktypeModelsFlattened.Select(paw => paw.ProjectId).Distinct().ToList();
            List<ProjectSelectionDto> finalSelectionList = [];

            foreach (Guid projectId in uniqueProjectIds)
            {
                List<ProjectAndWorktypeModelFlattened> projectItems = projectAndWorktypeModelsFlattened.FindAll(pwt => pwt.ProjectId == projectId);

                if (projectItems.Count == 0)
                {
                    continue;
                }

                ProjectSelectionDto projectSelectionDto = new()
                {
                    Id = projectId,
                    Name = projectItems[0].ProjectName,
                    WorkspaceId = projectItems[0].WorkspaceId,
                };

                List<ProjectWorktypeSelectionDto> projectWorktypes = [];

                projectWorktypes.AddRange(projectItems.Select(projectItem => new ProjectWorktypeSelectionDto
                {
                    Id = projectItem.WorktypeId,
                    Name = projectItem.WorktypeName
                }));

                projectSelectionDto.Worktypes = projectWorktypes;

                finalSelectionList.Add(projectSelectionDto);
            }

            return new GetProjectSelectionsForWorkspacesResponse
            {
                ProjectSelectionDtos = finalSelectionList
            };
        }
    }
}
