using BillIssue.Api.Business.Project;
using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Interfaces.TimeLogEntry;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Enums.Workspace;
using BillIssue.Shared.Models.Enums.Project;
using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Response.TimeLogEntry.Dto;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace BillIssue.Api.Business.TimeLogEntry
{
    public class TimeLogEntryFacade : ITimeLogEntryFacade
    {
        private readonly NpgsqlConnection _dbConnection;
        private readonly ILogger<TimeLogEntryFacade> _logger;

        private readonly ISessionFacade _sessionFacade;

        public TimeLogEntryFacade(
            ISessionFacade sessionFacade,
            NpgsqlConnection dbConnection,
            ILogger<TimeLogEntryFacade> logger)
        {
            _sessionFacade = sessionFacade;
            _dbConnection = dbConnection;
            _logger = logger;
        }

        #region Time Log Entry

        public async Task<TimeLogEntryDto> GetTimeLogEntry(string sessionId, GetTimeLogEntryRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            return GetTimeLogEntryWithPermissionCheck(sessionModel.Id, request.TimeLogEntryId, isAdmin: sessionModel.Role == UserRole.Admin);
        }

        public async Task<TimeLogEntryDto> CreateTimeLogEntry(string sessionId, CreateTimeLogEntryRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

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

            //NormaliseTimeLogEntryTimeAmounts(timeLogEntryDto);

            Guid newTimeLogEntryId = await CreateTimeLogEntryInTransaction(sessionModel.Id, sessionModel.Email, timeLogEntryDto, transaction);

            transaction.Commit();

            return await GetTimeLogEntry(sessionId, new GetTimeLogEntryRequest { TimeLogEntryId = newTimeLogEntryId });
        }

        public async Task<TimeLogEntryDto> ModifyTimeLogEntry(string sessionId, ModifyTimeLogEntryRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            TimeLogEntryDto timeLogEntryDto = await GetTimeLogEntry(sessionId, new GetTimeLogEntryRequest { TimeLogEntryId = request.TimeLogEntryId });

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

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            await ModifyTimeLogEntryInTransaction(sessionModel.Id, sessionModel.Email, timeLogEntryDto, transaction);

            transaction.Commit();

            return await GetTimeLogEntry(sessionId, new GetTimeLogEntryRequest { TimeLogEntryId = request.TimeLogEntryId });
        }

        public async Task RemoveTimeLogEntry(string sessionId, RemoveTimeLogEntryRequest request)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            TimeLogEntryDto timeLogEntryDto = await GetTimeLogEntry(sessionId, new GetTimeLogEntryRequest { TimeLogEntryId = request.TimeLogEntryId });

            NpgsqlTransaction transaction = _dbConnection.BeginTransaction();

            await MarkTimeLogEntryAsDeleted(sessionModel.Id, sessionModel.Email, timeLogEntryDto, transaction);

            transaction.Commit();
        }

        #endregion

        #region Private functions

        private async Task<Guid> CreateTimeLogEntryInTransaction(Guid userId, string userEmail, TimeLogEntryDto timeLogEntryDto, NpgsqlTransaction transaction)
        {
            Guid newTimeLogEntryId = Guid.NewGuid();
            try
            {
                await using NpgsqlCommand insertTimeLogEntry = new NpgsqlCommand(@"INSERT INTO 
                    time_log_entries (id, workspace_id, project_id, project_worktype_id, user_id, title, log_date, start_time, end_time, work_description, hour_amount, minute_amount, seconds_total_amount, created_by) 
                    VALUES (@id, @cwi, @pi, @pwi, @ui, @title, @logDate, @startTime, @endTime, @workDescription, @hourAmount, @minuteAmount, @secondTotalAmount, @createdBy)", _dbConnection, transaction)
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
                transaction.Rollback();

                throw new WorkspaceException("Failed to create time log entry", ExceptionCodes.TIME_LOG_ENTRY_FAILED_TO_CREATE);
            }
        }

        /*
         minimalWorkspaceRoleToAccess = What minimal role the user with id (userId) needs to have in the Company Workspace to access the time log entry
         minimalProjectRoleToAccess = Whet minimal role the user with id (userId) needs to have in the Project to access the time log entry (If the Company role is too low)
         */
        private TimeLogEntryDto GetTimeLogEntryWithPermissionCheck(Guid userId, Guid timeLogEntryId, WorkspaceUserRole minimalWorkspaceRoleToAccess = WorkspaceUserRole.Manager, ProjectUserRoles minimalProjectRoleToAccess = ProjectUserRoles.TeamLead, bool isAdmin = false)
        {
            var dictionary = new Dictionary<string, object> { { "@tlei", timeLogEntryId } };

            TimeLogEntryDto timeLogEntryDto = _dbConnection.Query<TimeLogEntryDto>(
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
            ", dictionary).FirstOrDefault();

            if (timeLogEntryDto == null) {
                _logger.LogError($"User with user id: {userId} tried to get an unknow or deleted time log entry with id: {timeLogEntryId}.");
                throw new ProjectException("Time log entry not found", ExceptionCodes.TIME_LOG_ENTRY_NOT_FOUND);
            }

            //If the owner of the entry or an admin is getting it, skip checks
            if (timeLogEntryDto.UserId == userId || isAdmin)
            {
                return timeLogEntryDto;
            }

            TimeLogEntryRoleContextDto timeLogEntryContextDto = GetUserRoleInTimeLogEntryContext(userId, timeLogEntryId);

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

        private TimeLogEntryRoleContextDto GetUserRoleInTimeLogEntryContext(Guid userId, Guid timeLogEntryId)
        {
            var dictionary = new Dictionary<string, object> { { "@ui", userId }, { "@tlei", timeLogEntryId } };

            TimeLogEntryRoleContextDto timeLogEntryRoleContextDto = _dbConnection.Query<TimeLogEntryRoleContextDto>(
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
	                tle.id = @tlei", dictionary).FirstOrDefault();

            return timeLogEntryRoleContextDto != null ? timeLogEntryRoleContextDto : new TimeLogEntryRoleContextDto();
        }

        private async Task ModifyTimeLogEntryInTransaction(Guid userId, string userEmail, TimeLogEntryDto timeLogEntryDto, NpgsqlTransaction transaction)
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
                    WHERE id = @targetTimeLogEntryId", _dbConnection, transaction)
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
                transaction.Rollback();
            }
        }

        private async Task MarkTimeLogEntryAsDeleted(Guid userId, string email, TimeLogEntryDto timeLogEntryDto, NpgsqlTransaction transaction)
        {
            try
            {
                await using NpgsqlCommand updateTimeLogEntry = new NpgsqlCommand(@"
                    UPDATE time_log_entries 
                    SET is_deleted = true
                    WHERE id = @targetTimeLogEntryId", _dbConnection, transaction)
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
                transaction.Rollback();
            }
        }

        private void NormaliseTimeLogEntryTimeAmounts(TimeLogEntryDto timeLogEntryDto)
        {
            //If hour, minute and second amounts are present the value is melformed, remove the hour and minute amounts for further calculations, but log a warning
            if (timeLogEntryDto.SecondsTotalAmount > 0 && timeLogEntryDto.HourAmount > 0 && timeLogEntryDto.MinuteAmount > 0)
            {
                timeLogEntryDto.HourAmount = 0;
                timeLogEntryDto.MinuteAmount = 0;
                _logger.LogWarning($"There was a melformed time log entry pushed to time log entry normalization function, removed the hours and minutes to fix the entry but the original values were as follows hours: {timeLogEntryDto.HourAmount} minutes: {timeLogEntryDto.MinuteAmount}");
            }

            //Users are not allowed to enter seconds into the time entry, so if seconds have any values we need to convert them to hours and minutes (because the timer function was used to calculate the amount)
            if (timeLogEntryDto.SecondsTotalAmount > 0 && timeLogEntryDto.HourAmount == 0 && timeLogEntryDto.MinuteAmount == 0)
            {
                timeLogEntryDto.HourAmount = timeLogEntryDto.SecondsTotalAmount / 3600;
                timeLogEntryDto.MinuteAmount = timeLogEntryDto.SecondsTotalAmount / 60;
            }

            //If hours and minutes are present but no seconds are in the dto, the values were adjusted or the manual input function was used to creat this entry
            if (timeLogEntryDto.SecondsTotalAmount == 0)
            {
                timeLogEntryDto.SecondsTotalAmount = timeLogEntryDto.HourAmount * 3600 + timeLogEntryDto.MinuteAmount * 60;
            }
        }

        #endregion
    }
}
