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
    public class GetProjectsForUserOperation : BaseOperation<GetProjectsForUserRequest, GetProjectsForUserResponse>
    {
        private readonly IProjectRepository _projectRepository;

        public GetProjectsForUserOperation(
            ILogger<GetProjectsForUserOperation> logger, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            OperationFactory operationFactory, 
            IValidator<GetProjectsForUserRequest> validator,
            IProjectRepository projectRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _projectRepository = projectRepository;
        }

        protected override async Task<GetProjectsForUserResponse> Execute(GetProjectsForUserRequest request, IUnitOfWork unitOfWork)
        {
            List<ProjectDto> projectDtos = await _projectRepository.GetAllProjectsWhereUserIsAssigned(request.SessionUserData.Id, unitOfWork);

            return new GetProjectsForUserResponse
            {
                ProjectDtos = projectDtos,
            };
        }
    }
}
