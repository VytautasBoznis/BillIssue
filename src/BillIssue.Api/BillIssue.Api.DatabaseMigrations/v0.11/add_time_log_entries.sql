CREATE TABLE IF NOT EXISTS time_log_entries (
   id uuid DEFAULT gen_random_uuid() PRIMARY KEY,
   workspace_id uuid REFERENCES workspace_workspaces(id),
   project_id uuid REFERENCES project_projects(id),
   project_worktype_id uuid REFERENCES project_worktypes(id),
   user_id uuid REFERENCES user_users(id),
   log_date DATE NUll,
   start_time TIME NUll,
   end_time TIME NUll,
   work_description VARCHAR(5120) NUll,
   hour_amount INTEGER NUll,
   minute_amount INTEGER NUll,
   seconds_total_amount BIGINT NUll,
   created_on TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
   created_by VARCHAR(512) NOT NULL,
   modified_on TIMESTAMP WITH TIME ZONE,
   modified_by VARCHAR(512)
);