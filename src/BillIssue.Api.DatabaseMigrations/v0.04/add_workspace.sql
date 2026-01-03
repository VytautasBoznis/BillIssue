CREATE TABLE IF NOT EXISTS workspace_workspaces (
   id uuid DEFAULT gen_random_uuid() PRIMARY KEY,
   creator_user_id uuid REFERENCES user_users(id),
   name VARCHAR(512) NOT NULL,
   description VARCHAR(5120) NUll,
   created_on TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
   created_by VARCHAR(512) NOT NULL,
   modified_on TIMESTAMP WITH TIME ZONE,
   modified_by VARCHAR(512)
);