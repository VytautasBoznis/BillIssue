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
    public class ModifyProjectWorktypeOperation : BaseOperation<ModifyProjectWorktypeRequest, ModifyProjectWorktypeResponse>
    {
        private readonly IProjectRepository _projectRepository;

        public ModifyProjectWorktypeOperation(
            ILogger<ModifyProjectWorktypeOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory, 
            OperationFactory operationFactory, 
            IValidator<ModifyProjectWorktypeRequest> validator,
            IProjectRepository projectRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _projectRepository = projectRepository;
        }

        protected override async Task<ModifyProjectWorktypeResponse> Execute(ModifyProjectWorktypeRequest request, IUnitOfWork unitOfWork)
        {
            ProjectDto projectDto = await _projectRepository.GetProjectDataWithPermissionCheck(request.SessionUserData.Id, request.ProjectId, ProjectUserRoles.Owner, request.SessionUserData.Role == UserRole.Admin, unitOfWork);

            if (projectDto == null)
            {
                _logger.LogError($"There was an issue loading the project with id: {request.ProjectId} for user with id: {request.SessionUserData.Id} .");
                throw new ProjectException("Project not found", ExceptionCodes.PROJECT_NOT_FOUND);
            }

            await unitOfWork.BeginTransactionAsync();

            ProjectWorktypeDto newProjectWorktypeValues = new ProjectWorktypeDto { ProjectId = request.ProjectId, ProjectWorktypeId = request.ProjectWorktypeId, Name = request.Name, Description = request.Description, IsBillable = request.IsBillable };
            await _projectRepository.ModifyProjectWorktypeInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, newProjectWorktypeValues, unitOfWork);

            await unitOfWork.CommitAsync();

            return new ModifyProjectWorktypeResponse();
        }
    }
}
