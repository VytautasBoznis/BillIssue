CREATE TABLE IF NOT EXISTS workspace_notifications (
   id uuid DEFAULT gen_random_uuid() PRIMARY KEY,
   workspace_id uuid REFERENCES workspace_workspaces(id),
   email VARCHAR(512) NOT NULL,
   created_on TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
   created_by VARCHAR(512) NOT NULL
);