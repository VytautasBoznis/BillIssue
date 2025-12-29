using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.TimeLogEntry;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Enums.Project;
using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Response.TimeLogEntry.Dto;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace BillIssue.Api.Business.Repositories.TimeLogEntry
{
    public class TimeLogEntryRepository : ITimeLogEntryRepository
    {
        private readonly ILogger<TimeLogEntryRepository> _logger;

        public TimeLogEntryRepository(ILogger<TimeLogEntryRepository> logger) 
        { 
            _logger = logger;
        }

        /*
         minimalWorkspaceRoleToAccess = What minimal role the user with id (userId) needs to have in the Company Workspace to access the time log entry
         minimalProjectRoleToAccess = Whet minimal role the user with id (userId) needs to have in the Project to access the time log entry (If the Company role is too low)
         */
        public async Task<TimeLogEntryDto> GetTimeLogEntryWithPermissionCheck(
            Guid userId,
            Guid timeLogEntryId,
            IUnitOfWork unitOfWork,
            WorkspaceUserRole minimalWorkspaceRoleToAccess = WorkspaceUserRole.Manager,
            ProjectUserRoles minimalProjectRoleToAccess = ProjectUserRoles.TeamLead,
            bool isAdmin = false)
        {
            var dictionary = new Dictionary<string, object> { { "@tlei", timeLogEntryId } };

            TimeLogEntryDto timeLogEntryDto = await unitOfWork.Connection.QueryFirstOrDefaultAsync<TimeLogEntryDto>(
            @"SELECT 
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
	            tle.log_date as LogDate,
	            tle.start_time as StartTime,
	            tle.end_time as EndTime,
	            tle.hour_amount as HourAmount,
	            tle.minute_amount as MinuteAmount,
	            tle.seconds_total_amount as SecondsTotalAmount,
	            tle.work_description as WorkDescription
            FROM time_log_entries tle
	            JOIN workspace_workspaces ww
		            ON tle.workspace_id = ww.id
	            JOIN project_projects pp
		            ON tle.project_id = pp.id
	            JOIN project_worktypes pw
		            ON tle.project_worktype_id = pw.id
	            JOIN user_users uu
		            ON tle.user_id = uu.id
            WHERE
	            tle.id = @tlei
                AND tle.is_deleted = false
            ", dictionary);

            if (timeLogEntryDto == null)
            {
                _logger.LogError($"User with user id: {userId} tried to get an unknow or deleted time log entry with id: {timeLogEntryId}.");
                throw new ProjectException("Time log entry not found", ExceptionCodes.TIME_LOG_ENTRY_NOT_FOUND);
            }

            //If the owner of the entry or an admin is getting it, skip checks
            if (timeLogEntryDto.UserId == userId || isAdmin)
            {
                return timeLogEntryDto;
            }

            TimeLogEntryRoleContextDto timeLogEntryContextDto = await GetUserRoleInTimeLogEntryContext(userId, timeLogEntryId, unitOfWork);

            if (timeLogEntryContextDto == null)
            {
                _logger.LogError($"User with user id: {userId} tried to get a time log entry with id: {timeLogEntryId} that he did not have access to.");
                throw new ProjectException("Time log entry not found", ExceptionCodes.TIME_LOG_ENTRY_NOT_FOUND);
            }

            if (timeLogEntryContextDto.WorkspaceUserRole >= minimalWorkspaceRoleToAccess || timeLogEntryContextDto.ProjectUserRole >= minimalProjectRoleToAccess)
            {
                return timeLogEntryDto;
            }
            else
            {
                _logger.LogError($"User with user id: {userId} tried to get a time log entry with id: {timeLogEntryId} that he did not have access to.");
                throw new ProjectException("Time log entry not found", ExceptionCodes.TIME_LOG_ENTRY_NOT_FOUND);
            }
        }

        public async Task<TimeLogEntryRoleContextDto> GetUserRoleInTimeLogEntryContext(Guid userId, Guid timeLogEntryId, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@ui", userId }, { "@tlei", timeLogEntryId } };

            TimeLogEntryRoleContextDto timeLogEntryRoleContextDto = await unitOfWork.Connection.QueryFirstOrDefaultAsync<TimeLogEntryRoleContextDto>(
                @"SELECT
	                tle.id as TimeLogEntryId,
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
	                tle.id = @tlei", dictionary);

            return timeLogEntryRoleContextDto != null ? timeLogEntryRoleContextDto : new TimeLogEntryRoleContextDto();
        }

        public async Task<Guid> CreateTimeLogEntryInTransaction(Guid userId, string userEmail, TimeLogEntryDto timeLogEntryDto, IUnitOfWork unitOfWork)
        {
            Guid newTimeLogEntryId = Guid.NewGuid();
            try
            {
                await using NpgsqlCommand insertTimeLogEntry = new NpgsqlCommand(@"INSERT INTO 
                    time_log_entries (id, workspace_id, project_id, project_worktype_id, user_id, title, log_date, start_time, end_time, work_description, hour_amount, minute_amount, seconds_total_amount, created_by) 
                    VALUES (@id, @cwi, @pi, @pwi, @ui, @title, @logDate, @startTime, @endTime, @workDescription, @hourAmount, @minuteAmount, @secondTotalAmount, @createdBy)", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@id", newTimeLogEntryId),
                        new("@cwi", timeLogEntryDto.WorkspaceId),
                        new("@pi", timeLogEntryDto.ProjectId),
                        new("@pwi", timeLogEntryDto.ProjectWorktypeId),
                        new("@ui", userId),
                        new("@title", timeLogEntryDto.Title),
                        new("@logDate", timeLogEntryDto.LogDate),
                        new("@startTime", NpgsqlDbType.Time) { Value = timeLogEntryDto.StartTime.HasValue ? timeLogEntryDto.StartTime : DBNull.Value },
                        new("@endTime", NpgsqlDbType.Time) { Value = timeLogEntryDto.EndTime.HasValue ? timeLogEntryDto.EndTime : DBNull.Value },
                        new("@workDescription", timeLogEntryDto.WorkDescription),
                        new("@hourAmount", timeLogEntryDto.HourAmount),
                        new("@minuteAmount", timeLogEntryDto.MinuteAmount),
                        new("@secondTotalAmount", timeLogEntryDto.SecondsTotalAmount),
                        new("@createdBy", userEmail),
                    }
                };

                await insertTimeLogEntry.ExecuteNonQueryAsync();

                _logger.LogInformation($"New time log entry created with description {timeLogEntryDto.WorkDescription} for userId: {userId}");

                return newTimeLogEntryId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to create a time log entry for user with id: {userId} in company workspace id: {timeLogEntryDto.WorkspaceId} and project id: {timeLogEntryDto.ProjectWorktypeId} exception message: {ex.Message} 

                     stacktrace: {ex.StackTrace}
                    """);
                await unitOfWork.RollbackAsync();

                throw new WorkspaceException("Failed to create time log entry", ExceptionCodes.TIME_LOG_ENTRY_FAILED_TO_CREATE);
            }
        }

        public async Task ModifyTimeLogEntryInTransaction(Guid userId, string userEmail, TimeLogEntryDto timeLogEntryDto, IUnitOfWork unitOfWork)
        {
            try
            {
                await using NpgsqlCommand updateTimeLogEntry = new NpgsqlCommand(@"
                    UPDATE time_log_entries 
                    SET title = @newTitle,
                        project_id = @newProjectId,
                        project_worktype_id = @newProjectWorktypeId, 
                        log_date = @newLogDate, 
                        start_time = @newStartTime, 
                        end_time = @newEndTime,
                        work_description = @newWorkDescription, 
                        hour_amount = @newHourAmount,
                        minute_amount = @newMinuteAmount, 
                        seconds_total_amount = @newSecondsTotalAmount, 
                        modified_on = @modifiedOn, 
                        modified_by = @modifiedBy 
                    WHERE id = @targetTimeLogEntryId", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@newTitle", timeLogEntryDto.Title),
                        new("@targetTimeLogEntryId", timeLogEntryDto.TimeLogEntryId),
                        new("@newProjectId", timeLogEntryDto.ProjectId),
                        new("@newProjectWorktypeId", timeLogEntryDto.ProjectWorktypeId),
                        new("@newLogDate", timeLogEntryDto.LogDate),
                        new("@newStartTime", NpgsqlDbType.Time) { Value = timeLogEntryDto.StartTime.HasValue ? timeLogEntryDto.StartTime : DBNull.Value },
                        new("@newEndTime", NpgsqlDbType.Time) { Value = timeLogEntryDto.EndTime.HasValue ? timeLogEntryDto.EndTime : DBNull.Value },
                        new("@newWorkDescription", timeLogEntryDto.WorkDescription),
                        new("@newHourAmount", timeLogEntryDto.HourAmount),
                        new("@newMinuteAmount", timeLogEntryDto.MinuteAmount),
                        new("@newSecondsTotalAmount", timeLogEntryDto.SecondsTotalAmount),
                        new("@modifiedBy", userEmail),
                        new("@modifiedOn", DateTime.UtcNow)
                    }
                };

                await updateTimeLogEntry.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to upadete time log entry: {timeLogEntryDto.TimeLogEntryId} user id: {userId} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                await unitOfWork.RollbackAsync();
            }
        }

        public async Task MarkTimeLogEntryAsDeleted(Guid userId, string email, TimeLogEntryDto timeLogEntryDto, IUnitOfWork unitOfWork)
        {
            try
            {
                await using NpgsqlCommand updateTimeLogEntry = new NpgsqlCommand(@"
                    UPDATE time_log_entries 
                    SET is_deleted = true
                    WHERE id = @targetTimeLogEntryId", unitOfWork.Connection, unitOfWork.Transaction)
                {
                    Parameters =
                    {
                        new("@targetTimeLogEntryId", timeLogEntryDto.TimeLogEntryId)
                    }
                };

                await updateTimeLogEntry.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"""
                    Failed to mark time log entry: {timeLogEntryDto.TimeLogEntryId} as deleted for user id: {userId} error: {ex.Message}

                    StackTrace: {ex.StackTrace}
                    """);
                await unitOfWork.RollbackAsync();
            }
        }

        public async Task<TimeLogEntryRoleContextForSearchDto> CheckIfUserHasAccess(Guid userId, Guid workspaceId, Guid? projectId, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@ui", userId }, { "@wi", workspaceId } };

            TimeLogEntryRoleContextForSearchDto timeLogEntryRoleContextForSearchDto = null;

            if (projectId == null)
            {
                timeLogEntryRoleContextForSearchDto = await unitOfWork.Connection.QueryFirstOrDefaultAsync<TimeLogEntryRoleContextForSearchDto>(
                @"SELECT
	                ww.name as WorkspaceName,
	                cwua.workspace_role as WorkspaceRole
                FROM time_log_entries tle
	                JOIN public.workspace_workspaces ww
		                ON tle.workspace_id = ww.id
	                LEFT JOIN workspace_user_assignments cwua
		                ON cwua.user_id = @ui
                WHERE
	                ww.id = @wi", dictionary);

                return timeLogEntryRoleContextForSearchDto != null ? timeLogEntryRoleContextForSearchDto : new TimeLogEntryRoleContextForSearchDto();
            }

            dictionary.Add("@pi", projectId);

            timeLogEntryRoleContextForSearchDto = await unitOfWork.Connection.QueryFirstOrDefaultAsync<TimeLogEntryRoleContextForSearchDto>(
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
                    AND pp.id = @pi", dictionary);

            return timeLogEntryRoleContextForSearchDto != null ? timeLogEntryRoleContextForSearchDto : new TimeLogEntryRoleContextForSearchDto();
        }

        public async Task<List<TimeLogEntryDto>> GetSearchResultsForQuery(SearchTimeLogEntriesRequest request, IUnitOfWork unitOfWork)
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
	            pw.is_billable as IsBillable,
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

            if (request.ProjectId != null)
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
                dictionary.Add("@sc", "%" + request.SearchContent + "%");
                searchQuery += "AND tle.title ilike @sc \n";
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

            searchQuery += "AND tle.log_date >= @sdate::date ";
            searchQuery += "AND tle.log_date <= @edate::date";

            IEnumerable<TimeLogEntryDto> timeLogEntries = await unitOfWork.Connection.QueryAsync<TimeLogEntryDto>(searchQuery, dictionary);

            return timeLogEntries.AsList();
        }

        public async Task<List<TimeLogEntryDto>> GetTimelogEntriesInDateRangeForUser(Guid userId, Guid WorkspaceId, DateTime startTime, DateTime endTime, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@ui", userId }, { "@ci", WorkspaceId }, { "@st", startTime.AddDays(-1) }, { "@et", endTime.Date } };

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


            IEnumerable<TimeLogEntryDto> timeLogEntries = await unitOfWork.Connection.QueryAsync<TimeLogEntryDto>(searchQuery, dictionary);
            return timeLogEntries.AsList();
        }
    }
}
