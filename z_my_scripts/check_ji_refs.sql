-- Check JobRequests for affected JIs
SELECT "Id", "JobInstanceId", "Status" FROM "JobRequests" WHERE "JobInstanceId" IN (1039, 1024, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011, 1012, 1013, 1025, 1026, 1027, 1028, 1029, 1030, 1031, 1032, 1033, 1034, 1035, 1036);

-- Check ReassignmentRecords for affected JIs
SELECT "Id", "ReassignJobInstanceId" FROM "ReassignmentRecords" WHERE "ReassignJobInstanceId" IN (1039, 1024, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011, 1012, 1013, 1025, 1026, 1027, 1028, 1029, 1030, 1031, 1032, 1033, 1034, 1035, 1036);

-- Check JobInstances self-ref (JobInstanceId column) for affected JIs
SELECT "Id", "JobInstanceId" FROM "JobInstances" WHERE "JobInstanceId" IN (1039, 1024, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011, 1012, 1013, 1025, 1026, 1027, 1028, 1029, 1030, 1031, 1032, 1033, 1034, 1035, 1036);
