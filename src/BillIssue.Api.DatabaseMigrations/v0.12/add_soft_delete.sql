ALTER TABLE project_projects
ADD COLUMN is_deleted BOOLEAN DEFAULT FALSE;

ALTER TABLE project_worktypes
ADD COLUMN is_deleted BOOLEAN DEFAULT FALSE;

ALTER TABLE workspace_workspaces
ADD COLUMN is_deleted BOOLEAN DEFAULT FALSE;