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
    public class GetWeekOfTimeEntriesOperation : BaseOperation<GetWeekOfTimeEntriesRequest, GetWeekOfTimeEntriesResponse>
    {
        private readonly ITimeLogEntryRepository _timeLogEntryRepository;

        private readonly Dictionary<DayOfWeek, int> _dayOfWeekCorrection = new()
        {
            { DayOfWeek.Monday, 0 },
            { DayOfWeek.Tuesday, 1 },
            { DayOfWeek.Wednesday, 2 },
            { DayOfWeek.Thursday, 3 },
            { DayOfWeek.Friday, 4 },
            { DayOfWeek.Saturday, 5 },
            { DayOfWeek.Sunday, 6 }
        };

        public GetWeekOfTimeEntriesOperation(
            ILogger<GetWeekOfTimeEntriesOperation> logger, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            OperationFactory operationFactory, 
            IValidator<GetWeekOfTimeEntriesRequest> validator,
            ITimeLogEntryRepository timeLogEntryRepository) : base(logger, unitOfWorkFactory, operationFactory, validator)
        {
            _timeLogEntryRepository = timeLogEntryRepository;
        }

        protected override async Task<GetWeekOfTimeEntriesResponse> Execute(GetWeekOfTimeEntriesRequest request, IUnitOfWork unitOfWork)
        {
            KeyValuePair<DayOfWeek, int> resultDayCorrection = _dayOfWeekCorrection.FirstOrDefault(dowc => dowc.Key == request.TargetDay.DayOfWeek);

            DateTime startOfWeek = request.TargetDay.AddDays(-1 * resultDayCorrection.Value);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            List<TimeLogEntryDto> timelogEntriesInRange = await _timeLogEntryRepository.GetTimelogEntriesInDateRangeForUser(request.SessionUserData.Id, request.WorkspaceId, startOfWeek, endOfWeek, unitOfWork);
            List<TimeLogEntriesForDay> timelogEntriesGrouped = GroupTimeEntriesByDay(timelogEntriesInRange, startOfWeek, endOfWeek);

            return new GetWeekOfTimeEntriesResponse
            {
                TimeLogEntriesForWeek = timelogEntriesGrouped
            };
        }

        private List<TimeLogEntriesForDay> GroupTimeEntriesByDay(List<TimeLogEntryDto> timeLogEntries, DateTime startOfWeek, DateTime endOfWeek)
        {
            List<DateTime> distinctDates = timeLogEntries.Where(tle => tle.LogDate.HasValue).Select(tle => tle.LogDate.Value).Distinct().ToList();
            List<TimeLogEntriesForDay> timeLogEntriesGrouped = [];

            for (int i = 0; i < 7; i++)
            {
                TimeLogEntriesForDay newDay = new()
                {
                    Day = DateTime.Parse(startOfWeek.AddDays(i).ToString("yyyy-MM-dd")),
                    TimeLogEntries = new List<TimeLogEntryDto>()
                };
                timeLogEntriesGrouped.Add(newDay);
            }

            foreach (var date in distinctDates)
            {
                TimeLogEntriesForDay targetTimeLogEntry = timeLogEntriesGrouped.FirstOrDefault(tle => tle.Day == date);

                List<TimeLogEntryDto> timeEntriesForDay = timeLogEntries.Where(tle => tle.LogDate == date).ToList();
                targetTimeLogEntry.TimeLogEntries = timeEntriesForDay;
                targetTimeLogEntry.SecondsLogged = timeEntriesForDay.Sum(tle => tle.SecondsTotalAmount.Value);
            }

            return timeLogEntriesGrouped;
        }
    }
}
