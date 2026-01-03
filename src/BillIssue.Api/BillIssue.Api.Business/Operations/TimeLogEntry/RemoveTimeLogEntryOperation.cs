using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.TimeLogEntry;
using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Response.TimeLogEntry;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.TimeLogEntry
{
    public class RemoveTimeLogEntryOperation : BaseOperation<RemoveTimeLogEntryRequest, RemoveTimeLogEntryResponse>
    {
        private readonly ITimeLogEntryRepository _timeLogEntryRepository;

        public RemoveTimeLogEntryOperation(
            ILogger<RemoveTimeLogEntryOperation> logger,
            IUnitOfWorkFactory unitOfWorkFactory, 
            OperationFactory operationFactory, 
            IValidator<RemoveTimeLogEntryRequest> validator,
            ITimeLogEntryRepository timeLogEntryRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _timeLogEntryRepository = timeLogEntryRepository;
        }

        protected override async Task<RemoveTimeLogEntryResponse> Execute(RemoveTimeLogEntryRequest request, IUnitOfWork unitOfWork)
        {
            GetTimeLogEntryRequest getTimeLogEntryRequest = new GetTimeLogEntryRequest
            {
                SessionUserData = request.SessionUserData,
                CreatedFromController = false,
                TimeLogEntryId = request.TimeLogEntryId,
            };

            GetTimeLogEntryResponse getTimeLogEntryResponse = await _operationFactory
                                                                        .Get<GetTimeLogEntryOperation>()
                                                                        .Run(getTimeLogEntryRequest, unitOfWork);

            await unitOfWork.BeginTransactionAsync();

            await _timeLogEntryRepository.MarkTimeLogEntryAsDeleted(request.SessionUserData.Id, request.SessionUserData.Email, getTimeLogEntryResponse.TimeLogEntryDto, unitOfWork);

            await unitOfWork.CommitAsync();

            return new RemoveTimeLogEntryResponse();
        }
    }
}
