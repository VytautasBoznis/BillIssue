using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Project;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Enums.Project;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Response.Project;
using BillIssue.Shared.Models.Response.Project.Dto;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.Project
{
    public class GetProjectWorktypeOperation : BaseOperation<GetProjectWorktypeRequest, GetProjectWorktypeResponse>
    {
        private readonly IProjectRepository _projectRepository;

        public GetProjectWorktypeOperation(
            ILogger<GetProjectWorktypeOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<GetProjectWorktypeRequest> validator,
            IProjectRepository projectRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _projectRepository = projectRepository;
        }

        protected override async Task<GetProjectWorktypeResponse> Execute(GetProjectWorktypeRequest request, IUnitOfWork unitOfWork)
        {
            ProjectWorktypeDto projectWorktypeDto = await _projectRepository.GetProjectWorktypeWithPermissionCheck(request.SessionUserData.Id, request.ProjectWorktypeId, ProjectUserRoles.TeamLead, request.SessionUserData.Role == UserRole.Admin, unitOfWork);

            if (projectWorktypeDto == null)
            {
                _logger.LogError($"There was an issue loading the project worktype with id: {request.ProjectWorktypeId} for user with id: {request.SessionUserData.Id} .");
                throw new ProjectException("Project worktype not found", ExceptionCodes.PROJECT_WORKTYPE_NOT_FOUND);
            }

            return new GetProjectWorktypeResponse()
            {
                ProjectWorktypeDto = projectWorktypeDto
            };
        }
    }
}
