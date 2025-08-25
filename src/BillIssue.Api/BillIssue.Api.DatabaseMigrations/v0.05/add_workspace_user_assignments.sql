CREATE TABLE IF NOT EXISTS workspace_user_assignments (
   id uuid DEFAULT gen_random_uuid() PRIMARY KEY,
   workspace_id uuid REFERENCES workspace_workspaces(id),
   user_id uuid REFERENCES user_users(id),
   workspace_role INTEGER NOT NULL DEFAULT 1,
   created_on TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
   created_by VARCHAR(512) NOT NULL,
   modified_on TIMESTAMP WITH TIME ZONE,
   modified_by VARCHAR(512)
);