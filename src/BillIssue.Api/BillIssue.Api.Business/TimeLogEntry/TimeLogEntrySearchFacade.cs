using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Interfaces.TimeLogEntry;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Response.TimeLogEntry.Dto;
using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BillIssue.Api.Business.TimeLogEntry
{
    public class TimeLogEntrySearchFacade : ITimeLogEntrySearchFacade
    {
        private readonly NpgsqlConnection _dbConnection;
        private readonly ILogger<TimeLogEntrySearchFacade> _logger;

        private readonly ISessionFacade _sessionFacade;

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

        public TimeLogEntrySearchFacade(
            ISessionFacade sessionFacade,
            NpgsqlConnection dbConnection,
            ILogger<TimeLogEntrySearchFacade> logger)
        {
            _sessionFacade = sessionFacade;
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<List<TimeLogEntryDto>> SearchTimeLogEntries(string sessionId, SearchTimeLogEntriesRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            TimeLogEntryRoleContextForSearchDto timeLogEntryRoleContextForSearchDto = CheckIfUserHasAccess(sessionModel.Id, request.WorkspaceId, request.ProjectId);

            if (CheckIfUserEligibleForSearch(timeLogEntryRoleContextForSearchDto, sessionModel.Id, sessionModel.Role, request.UserId, request.ProjectId != null))
            {
                _logger.LogWarning($"Search failed for user because the user did not have enaugh permission to search for those time log entries user id: {sessionModel.Id}, searchRequest: {JsonConvert.SerializeObject(request)}");
                return new List<TimeLogEntryDto>();
            }

            return GetSearchResultsForQuery(request);
        }

        public async Task<List<TimeLogEntriesForDay>> GetWeekOfTimeEntries(string sessionId, GetWeekOfTimeEntriesRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            KeyValuePair<DayOfWeek, int> resultDayCorrection = _dayOfWeekCorrection.FirstOrDefault(dowc => dowc.Key == request.TargetDay.DayOfWeek);

            DateTime startOfWeek = request.TargetDay.AddDays(-1 * resultDayCorrection.Value);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            List<TimeLogEntryDto> timelogEntriesInRange = GetTimelogEntriesInDateRangeForUser(sessionModel.Id, request.WorkspaceId, startOfWeek, endOfWeek);
            List<TimeLogEntriesForDay> timelogEntriesGrouped = GroupTimeEntriesByDay(timelogEntriesInRange, startOfWeek, endOfWeek);

            return timelogEntriesGrouped;
        }

        #region Private Functions

        private List<TimeLogEntryDto> GetTimelogEntriesInDateRangeForUser(Guid userId, Guid WorkspaceId, DateTime startTime, DateTime endTime)
        {
            var dictionary = new Dictionary<string, object> { { "@ui", userId}, { "@ci", WorkspaceId }, { "@st", startTime}, { "@et", endTime } };

            string searchQuery = @"SELECT
	            tle.id as TimeLogEntryId,
	            ww.id as WorkspaceId,
	            ww.name as WorkspaceName,
	            pp.id as ProjectId,
	            pp.name as ProjectName,
	            pw.id as ProjectWorktypeId,
	            pw.name as ProjectWorktypeName,
	            uu.id as UserId,
	            uu.first_name as FirstName,
	            uu.last_name as LastName,
	            uu.email as Email,
                tle.title as Title,
	            tle.log_date as LogDate,
	            tle.start_time as StartTime,
	            tle.end_time as EndTime,
	            tle.hour_amount as HourAmount,
	            tle.minute_amount as MinuteAmount,
	            tle.seconds_total_amount as SecondsTotalAmount,
	            tle.work_description as WorkDescription
            FROM time_log_entries tle
	            JOIN workspace_workspaces ww
		            ON ww.id = tle.workspace_id
	            JOIN project_projects pp
		            ON pp.id = tle.project_id
	            JOIN project_worktypes pw
		            ON pw.id = tle.project_worktype_id
	            JOIN user_users uu
		            ON uu.id = tle.user_id
	        WHERE
                tle.workspace_id = @ci
                AND tle.user_id = @ui
                AND tle.is_deleted = false
                AND tle.log_date BETWEEN @st AND @et";

            List<TimeLogEntryDto> timeLogEntries = _dbConnection.Query<TimeLogEntryDto>(searchQuery, dictionary).ToList();
            return timeLogEntries;
        }

        private List<TimeLogEntryDto> GetSearchResultsForQuery(SearchTimeLogEntriesRequest request)
        {
            var dictionary = new Dictionary<string, object> { { "@ci", request.WorkspaceId } };

            string searchQuery = @"SELECT
	            tle.id as TimeLogEntryId,	
	            ww.id as WorkspaceId,
	            ww.name as WorkspaceName,
	            pp.id as ProjectId,
	            pp.name as ProjectName,
	            pw.id as ProjectWorktypeId,
	            pw.name as ProjectWorktypeName,
	            uu.id as UserId,
	            uu.first_name as FirstName,
	            uu.last_name as LastName,
	            uu.email as Email,
                tle.title as Title,
	            tle.log_date as LogDate,
	            tle.start_time as StartTime,
	            tle.end_time as EndTime,
	            tle.hour_amount as HourAmount,
	            tle.minute_amount as MinuteAmount,
	            tle.seconds_total_amount as SecondsTotalAmount,
	            tle.work_description as WorkDescription
            FROM time_log_entries tle
	            JOIN workspace_workspaces ww
		            ON ww.id = tle.workspace_id
	            JOIN project_projects pp
		            ON pp.id = tle.project_id
	            JOIN project_worktypes pw
		            ON pw.id = tle.project_worktype_id
	            JOIN user_users uu
		            ON uu.id = tle.user_id
	        WHERE
                tle.workspace_id = @ci
                AND tle.is_deleted = false
                ";

            if(request.ProjectId != null)
            {
                dictionary.Add("@pi", request.ProjectId);
                searchQuery += "AND tle.project_id = @pi \n";
            }

            if (request.ProjectWorktypeId != null)
            {
                dictionary.Add("@pwi", request.ProjectWorktypeId);
                searchQuery += "AND tle.project_worktype_id = @pwi \n";
            }

            if (request.UserId != null)
            {
                dictionary.Add("@ui", request.UserId);
                searchQuery += "AND tle.user_id = @ui \n";
            }

            if (!string.IsNullOrEmpty(request.SearchContent))
            {
                dictionary.Add("@sc", request.SearchContent);
                searchQuery += "AND tle.work_description like '%@sc%' \n";
            }

            DateTime endDate = request.EndDate ?? DateTime.Now;
            DateTime startDate = request.StartDate ?? DateTime.Now;

            if (!request.CustomTimeInterval)
            {
                switch (request.TimeLogSearchInterval)
                {
                    case Shared.Models.Enums.TimeLog.TimeLogSearchInterval.Day:
                        {
                            startDate.AddDays(-1);
                            break;
                        }
                    case Shared.Models.Enums.TimeLog.TimeLogSearchInterval.Week:
                        {
                            startDate.AddDays(-7);
                            break;
                        }
                    case Shared.Models.Enums.TimeLog.TimeLogSearchInterval.Month:
                        {
                            startDate.AddMonths(-1);
                            break;
                        }
                    case Shared.Models.Enums.TimeLog.TimeLogSearchInterval.Year:
                        {
                            startDate.AddYears(-1);
                            break;
                        }
                    default:
                        {
                            startDate.AddDays(-1);
                            break;
                        }

                }
            }

            dictionary.Add("@sdate", startDate.ToString("yyyy-MM-dd"));
            dictionary.Add("@edate", endDate.ToString("yyyy-MM-dd"));

            searchQuery += "AND tle.log_date BETWEEN '@sdate' AND '@edate'";

            List<TimeLogEntryDto> timeLogEntries = _dbConnection.Query<TimeLogEntryDto>(searchQuery, dictionary).ToList();

            return timeLogEntries != null ? timeLogEntries : new List<TimeLogEntryDto>();
        }

        public TimeLogEntryRoleContextForSearchDto CheckIfUserHasAccess(Guid userId, Guid workspaceId, Guid? projectId)
        {
            var dictionary = new Dictionary<string, object> { { "@ui", userId }, { "@wi", workspaceId } };

            TimeLogEntryRoleContextForSearchDto timeLogEntryRoleContextForSearchDto = null;

            if (projectId == null)
            {
                timeLogEntryRoleContextForSearchDto = _dbConnection.Query<TimeLogEntryRoleContextForSearchDto>(
                @"SELECT
	                ww.name as WorkspaceName,
	                cwua.workspace_role as WorkspaceRole
                FROM time_log_entries tle
	                JOIN public.workspace_workspaces ww
		                ON tle.workspace_id = ww.id
	                LEFT JOIN workspace_user_assignments cwua
		                ON cwua.user_id = @ui
                WHERE
	                ww.id = @wi", dictionary).FirstOrDefault();

                return timeLogEntryRoleContextForSearchDto != null ? timeLogEntryRoleContextForSearchDto : new TimeLogEntryRoleContextForSearchDto();
            }

            dictionary.Add("@pi", projectId);

            timeLogEntryRoleContextForSearchDto = _dbConnection.Query<TimeLogEntryRoleContextForSearchDto>(
                @"SELECT
	                pp.name as ProjectName,
	                pua.project_role as ProjectRole,
	                ww.name as WorkspaceName,
	                cwua.workspace_role as WorkspaceRole
                FROM time_log_entries tle
	                JOIN public.workspace_workspaces ww
		                ON tle.workspace_id = ww.id
	                JOIN project_projects pp 
		                ON tle.project_id = pp.id
	                LEFT JOIN workspace_user_assignments cwua
		                ON cwua.user_id = @ui
	                LEFT JOIN project_user_assignments pua
		                ON pua.user_id = @ui
                WHERE
	                ww.id = @wi
                    AND pp.id = @pi", dictionary).FirstOrDefault();

            return timeLogEntryRoleContextForSearchDto != null ? timeLogEntryRoleContextForSearchDto : new TimeLogEntryRoleContextForSearchDto();
        }

        private bool CheckIfUserEligibleForSearch(TimeLogEntryRoleContextForSearchDto timeLogEntryRoleContextForSearchDto, Guid userId, UserRole userRole, Guid? targetUserId, bool searchingInProject)
        {
            if (targetUserId != null && targetUserId == userId) // if searching for your entries allow any search
            {
                return true;
            }

            if (userRole == UserRole.Admin) // if admin is searching allow any search
            {
                return true;
            }

            if (searchingInProject && timeLogEntryRoleContextForSearchDto.ProjectUserRole >= Shared.Models.Enums.Project.ProjectUserRoles.TeamLead)
            {
                return true;
            }

            if (!searchingInProject && timeLogEntryRoleContextForSearchDto.WorkspaceUserRole >= Shared.Models.Enums.Workspace.WorkspaceUserRole.Manager)
            {
                return true;
            }

            return false;
        }

        private List<TimeLogEntriesForDay> GroupTimeEntriesByDay(List<TimeLogEntryDto> timeLogEntries, DateTime startOfWeek, DateTime endOfWeek)
        {
            List<DateTime> distinctDates = timeLogEntries.Where(tle => tle.LogDate.HasValue).Select(tle => tle.LogDate.Value).Distinct().ToList();
            List<TimeLogEntriesForDay> timeLogEntriesGrouped = [];

            for(int i = 0; i < 7; i++)
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

        #endregion
    }
}
