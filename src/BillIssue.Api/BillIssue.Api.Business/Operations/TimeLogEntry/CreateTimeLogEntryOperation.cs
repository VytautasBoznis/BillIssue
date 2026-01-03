using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.TimeLogEntry;
using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Response.TimeLogEntry;
using BillIssue.Shared.Models.Response.TimeLogEntry.Dto;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.TimeLogEntry
{
    public class CreateTimeLogEntryOperation : BaseOperation<CreateTimeLogEntryRequest, CreateTimeLogEntryResponse>
    {
        private readonly ITimeLogEntryRepository _timeLogEntryRepository;

        public CreateTimeLogEntryOperation(
            ILogger<CreateTimeLogEntryOperation> logger, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            OperationFactory operationFactory, 
            IValidator<CreateTimeLogEntryRequest> validator,
            ITimeLogEntryRepository timeLogEntryRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _timeLogEntryRepository = timeLogEntryRepository;
        }

        protected override async Task<CreateTimeLogEntryResponse> Execute(CreateTimeLogEntryRequest request, IUnitOfWork unitOfWork)
        {
            await unitOfWork.BeginTransactionAsync();

            TimeLogEntryDto timeLogEntryDto = new TimeLogEntryDto
            {
                WorkspaceId = request.WorkspaceId,
                ProjectId = request.ProjectId,
                ProjectWorktypeId = request.ProjectWorktypeId,
                LogDate = request.LogDate,
                Title = request.Title,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                HourAmount = request.HourAmount,
                MinuteAmount = request.MinuteAmount,
                SecondsTotalAmount = request.SecondsTotalAmount,
                WorkDescription = request.WorkDescription
            };

            Guid newTimeLogEntryId = await _timeLogEntryRepository.CreateTimeLogEntryInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, timeLogEntryDto, unitOfWork);

            await unitOfWork.CommitAsync();

            GetTimeLogEntryRequest getTimeLogEntryRequest = new GetTimeLogEntryRequest
            {
                SessionUserData = request.SessionUserData,
                CreatedFromController = false,
                TimeLogEntryId = newTimeLogEntryId,
            };

            GetTimeLogEntryResponse getTimeLogEntryResponse = await _operationFactory
                                                                        .Get<GetTimeLogEntryOperation>()
                                                                        .Run(getTimeLogEntryRequest, unitOfWork);

            CreateTimeLogEntryResponse response = new CreateTimeLogEntryResponse
            {
                TimeLogEntryDto = getTimeLogEntryResponse.TimeLogEntryDto
            };

            return response;
        }
    }
}
