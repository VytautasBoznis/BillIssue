CREATE TABLE IF NOT EXISTS user_confirmations (
   id uuid NOT NULL PRIMARY KEY,
   user_id uuid REFERENCES user_users(id),
   confirmation_type INTEGER NOT NULL,
   created_on TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
   created_by VARCHAR(512) NOT NULL,
   modified_on TIMESTAMP WITH TIME ZONE,
   modified_by VARCHAR(512)
);