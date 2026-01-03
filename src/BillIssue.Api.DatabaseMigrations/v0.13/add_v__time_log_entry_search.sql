CREATE VIEW 
	v__time_log_entry_search
AS
	SELECT 
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