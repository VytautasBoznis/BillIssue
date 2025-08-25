using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Interfaces.Schedule;
using BillIssue.Shared.Models.Enums.Schedule;
using BillIssue.Shared.Models.Request.Schedule;
using BillIssue.Shared.Models.Response.Schedule.Dto;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace BillIssue.Api.Business.Schedule
{
    public class ScheduleFacade : IScheduleFacade
    {
        private readonly NpgsqlConnection _dbConnection;
        private readonly ILogger<ScheduleFacade> _logger;

        private readonly ISessionFacade _sessionFacade;

        public ScheduleFacade(
            ISessionFacade sessionFacade,
            NpgsqlConnection dbConnection,
            ILogger<ScheduleFacade> logger)
        {
            _sessionFacade = sessionFacade;
            _dbConnection = dbConnection;
            _logger = logger;

        }

        #region Facade implementation

        public UserScheduleDto GetUserSchedule(string sessionid, GetUserScheduleRequest getUserSchedule)
        {
            return GetDefaultUserSchedule();
        }

        #endregion

        #region Private functions

        private UserScheduleDto GetDefaultUserSchedule()
        {
            List<DayScheduleDto> workWeekSchedule =
            [
                GetDefaultScheduleForADay(WorkWeekDayEnum.Monday),
                GetDefaultScheduleForADay(WorkWeekDayEnum.Tuesday),
                GetDefaultScheduleForADay(WorkWeekDayEnum.Wednesday),
                GetDefaultScheduleForADay(WorkWeekDayEnum.Thursday),
                GetDefaultScheduleForADay(WorkWeekDayEnum.Friday),
                GetDefaultScheduleForADay(WorkWeekDayEnum.Saturday),
                GetDefaultScheduleForADay(WorkWeekDayEnum.Sunday),
            ];

            return new UserScheduleDto
            {
                WorkWeekSchedule = workWeekSchedule,
            };
        }

        public DayScheduleDto GetDefaultScheduleForADay(WorkWeekDayEnum workWeekDay)
        {
            return new DayScheduleDto
            {
                WorkWeekDay = workWeekDay
            };
        }

        #endregion
    }
}
