CREATE TABLE IF NOT EXISTS multilanguage_items (
   multilanguage_item_id uuid DEFAULT gen_random_uuid() PRIMARY KEY,
   multilanguage_index_id uuid DEFAULT gen_random_uuid() REFERENCES multilanguage_indexes(multilanguage_index_id),
   language_type_id smallint NOT NULL,
   text VARCHAR(5000) NOT NULL,
   created_on TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
   created_by VARCHAR(255) NOT NULL,
   modified_on TIMESTAMP WITH TIME ZONE,
   modified_by VARCHAR(255)
);