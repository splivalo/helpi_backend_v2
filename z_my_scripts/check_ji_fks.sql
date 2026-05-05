-- Find ALL foreign key constraints referencing JobInstances, with their table names and delete rules
SELECT
    tc.constraint_name,
    tc.table_name AS source_table,
    kcu.column_name AS source_column,
    rc.delete_rule
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.referential_constraints AS rc ON tc.constraint_name = rc.constraint_name
JOIN information_schema.key_column_usage AS ccu ON rc.unique_constraint_name = ccu.constraint_name
WHERE ccu.table_name = 'JobInstances'
  AND tc.constraint_type = 'FOREIGN KEY'
ORDER BY rc.delete_rule, tc.table_name;
