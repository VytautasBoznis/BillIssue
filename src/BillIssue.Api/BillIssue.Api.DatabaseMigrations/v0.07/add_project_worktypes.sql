CREATE TABLE IF NOT EXISTS project_worktypes (
   id uuid DEFAULT gen_random_uuid() PRIMARY KEY,
   project_id uuid REFERENCES project_projects(id),
   name VARCHAR(512) NOT NULL,
   description VARCHAR(5120) NUll,
   created_on TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
   created_by VARCHAR(512) NOT NULL,
   modified_on TIMESTAMP WITH TIME ZONE,
   modified_by VARCHAR(512)
);