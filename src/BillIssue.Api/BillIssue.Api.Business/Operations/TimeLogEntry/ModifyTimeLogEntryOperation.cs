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
    public class ModifyTimeLogEntryOperation : BaseOperation<ModifyTimeLogEntryRequest, ModifyTimeLogEntryResponse>
    {
        private readonly ITimeLogEntryRepository _timeLogEntryRepository;

        public ModifyTimeLogEntryOperation(
            ILogger<ModifyTimeLogEntryOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            OperationFactory operationFactory,
            IValidator<ModifyTimeLogEntryRequest> validator,
            ITimeLogEntryRepository timeLogEntryRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _timeLogEntryRepository = timeLogEntryRepository;
        }

        protected override async Task<ModifyTimeLogEntryResponse> Execute(ModifyTimeLogEntryRequest request, IUnitOfWork unitOfWork)
        {
            GetTimeLogEntryRequest getTimeLogEntryRequest = new GetTimeLogEntryRequest
            {
                TimeLogEntryId = request.TimeLogEntryId,
            };

            GetTimeLogEntryResponse getTimeLogEntryResponse = await _operationFactory
                                                                        .Get<GetTimeLogEntryOperation>(typeof(GetTimeLogEntryOperation))
                                                                        .Run(getTimeLogEntryRequest, unitOfWork);

            TimeLogEntryDto timeLogEntryDto = getTimeLogEntryResponse.TimeLogEntryDto;

            timeLogEntryDto.ProjectId = request.ProjectId;
            timeLogEntryDto.ProjectWorktypeId = request.ProjectWorktypeId;
            timeLogEntryDto.Title = request.Title;
            timeLogEntryDto.LogDate = request.LogDate;
            timeLogEntryDto.StartTime = request.StartTime;
            timeLogEntryDto.EndTime = request.EndTime;
            timeLogEntryDto.HourAmount = request.HourAmount;
            timeLogEntryDto.MinuteAmount = request.MinuteAmount;
            timeLogEntryDto.SecondsTotalAmount = request.SecondsTotalAmount;
            timeLogEntryDto.WorkDescription = request.WorkDescription;

            await unitOfWork.BeginTransactionAsync();

            await _timeLogEntryRepository.ModifyTimeLogEntryInTransaction(request.SessionUserData.Id, request.SessionUserData.Email, timeLogEntryDto, unitOfWork);

            await unitOfWork.CommitAsync();


            GetTimeLogEntryResponse finalTimeLogEntryGetResponse = await _operationFactory
                                                                        .Get<GetTimeLogEntryOperation>(typeof(GetTimeLogEntryOperation))
                                                                        .Run(getTimeLogEntryRequest, unitOfWork);

            ModifyTimeLogEntryResponse response = new ModifyTimeLogEntryResponse 
            { 
                TimeLogEntryDto = finalTimeLogEntryGetResponse.TimeLogEntryDto 
            };

            return response;
        }
    }
}
