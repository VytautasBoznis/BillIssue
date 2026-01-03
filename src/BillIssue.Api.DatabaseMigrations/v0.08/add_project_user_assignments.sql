CREATE TABLE IF NOT EXISTS project_user_assignments (
   id uuid DEFAULT gen_random_uuid() PRIMARY KEY,
   project_id uuid REFERENCES project_projects(id),
   user_id uuid REFERENCES user_users(id),
   project_role INTEGER NOT NULL DEFAULT 1,
   created_on TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
   created_by VARCHAR(512) NOT NULL,
   modified_on TIMESTAMP WITH TIME ZONE,
   modified_by VARCHAR(512)
);