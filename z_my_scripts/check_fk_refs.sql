-- Check for any reviews linked to JIs being deleted
SELECT "Id", "JobInstanceId" FROM "Reviews" WHERE "JobInstanceId" IN (1039, 1024, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011, 1012, 1013, 1025, 1026, 1027, 1028, 1029, 1030, 1031, 1032, 1033, 1034, 1035, 1036);

-- Check FK constraints on JobInstances
SELECT
    tc.constraint_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name,
    rc.delete_rule
FROM information_schema.table_constraints AS tc
JOIN information_schema.referential_constraints AS rc ON tc.constraint_name = rc.unique_constraint_name
JOIN information_schema.constraint_column_usage AS ccu ON ccu.constraint_name = rc.constraint_name
WHERE tc.table_name = 'JobInstances' AND tc.constraint_type = 'PRIMARY KEY';
