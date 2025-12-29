using BillIssue.Api.Business.Base;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.TimeLogEntry;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Response.TimeLogEntry;
using BillIssue.Shared.Models.Response.TimeLogEntry.Dto;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Operations.TimeLogEntry
{
    public class GetTimeLogEntryOperation : BaseOperation<GetTimeLogEntryRequest, GetTimeLogEntryResponse>
    {
        private readonly ITimeLogEntryRepository _timeLogEntryRepository;

        public GetTimeLogEntryOperation(
            ILogger<GetTimeLogEntryOperation> logger, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            OperationFactory operationFactory, 
            IValidator<GetTimeLogEntryRequest> validator,
            ITimeLogEntryRepository timeLogEntryRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _timeLogEntryRepository = timeLogEntryRepository;
        }

        protected override async Task<GetTimeLogEntryResponse> Execute(GetTimeLogEntryRequest request, IUnitOfWork unitOfWork)
        {
            TimeLogEntryDto timeLogEntryDto = await _timeLogEntryRepository.GetTimeLogEntryWithPermissionCheck(request.SessionUserData.Id, request.TimeLogEntryId, unitOfWork, isAdmin: request.SessionUserData.Role == UserRole.Admin);

            return new GetTimeLogEntryResponse { 
                TimeLogEntryDto = timeLogEntryDto 
            };
        }
    }
}
