CREATE TABLE IF NOT EXISTS project_projects (
   id uuid DEFAULT gen_random_uuid() PRIMARY KEY,
   workspace_id uuid REFERENCES workspace_workspaces(id),
   name VARCHAR(512) NOT NULL,
   description VARCHAR(5120) NUll,
   created_on TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
   created_by VARCHAR(512) NOT NULL,
   modified_on TIMESTAMP WITH TIME ZONE,
   modified_by VARCHAR(512)
);