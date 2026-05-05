-- =====================================================================
-- Restore Order 35 (Tue 08:15-09:15 with Petra/SA 104) v2
-- Cleans up the broken cascade including ReassignmentRecords FK references.
-- =====================================================================
BEGIN;

-- 0. Inspect & clean ReassignmentRecords pointing to soon-to-be-deleted JI/SA
SELECT 'ReassignmentRecords for order 35' AS section;
SELECT * FROM "ReassignmentRecords"
WHERE "OrderId" = 35
   OR "ReassignJobInstanceId" IN (SELECT "Id" FROM "JobInstances" WHERE "OrderId"=35)
   OR "ReassignAssignmentId" IN (SELECT "Id" FROM "ScheduleAssignments" WHERE "OrderId"=35)
   OR "CurrentAssignmentId" IN (SELECT "Id" FROM "ScheduleAssignments" WHERE "OrderId"=35)
ORDER BY "Id";

-- 1. Delete ReassignmentRecords that reference our order's JI/SA
DELETE FROM "ReassignmentRecords"
WHERE "OrderId" = 35
   OR "ReassignJobInstanceId" IN (SELECT "Id" FROM "JobInstances" WHERE "OrderId"=35)
   OR "ReassignAssignmentId" IN (SELECT "Id" FROM "ScheduleAssignments" WHERE "OrderId"=35)
   OR "CurrentAssignmentId" IN (SELECT "Id" FROM "ScheduleAssignments" WHERE "OrderId"=35);

-- 1b. Clean CouponUsages referencing JIs we're about to delete
DELETE FROM "CouponUsages"
WHERE "JobInstanceId" IN (
  1014, 1020, 1056, 1072, 1023, 1055, 1071,
  1069, 1070, 1073, 1074, 1075, 1076, 1077, 1078, 1079, 1080, 1081, 1082, 1083, 1084,
  1053, 1054, 1057, 1058, 1059, 1060, 1061, 1062, 1063, 1064, 1065, 1066, 1067
);

-- 2. Null out FK references inside JobInstances themselves
UPDATE "JobInstances"
SET "ScheduleAssignmentId" = NULL,
    "PrevAssignmentId" = NULL,
    "RescheduledToId" = NULL,
    "RescheduledAt" = NULL,
    "NeedsSubstitute" = false
WHERE "OrderId" = 35;

-- 3. Delete duplicate / stub job instances (keep one canonical row per scheduled date)
-- These are duplicates created during cancel/restore experiments
DELETE FROM "JobInstances"
WHERE "Id" IN (
  -- 2026-05-04 secondary stub (10:15 sub session)
  1014,
  -- 2026-05-12 stubs/duplicates
  1020, 1056, 1072,
  -- 2026-05-05 sub copies (keep 1023 as the canonical Tue 05.05)
  1055, 1071,
  -- All sub-assignment 120 future copies (we collapse to assignment 104 series)
  1069, 1070, 1073, 1074, 1075, 1076, 1077, 1078, 1079, 1080, 1081, 1082, 1083, 1084,
  -- All sub-assignment 119 future copies (collapse to 104 series; 1023..1067)
  1053, 1054, 1057, 1058, 1059, 1060, 1061, 1062, 1063, 1064, 1065, 1066, 1067,
  -- Assignment 117 / 111 historic duplicates (keep 994/995/996 from SA 104)
  1021, 1022, 1037, 1038, 1023
);

-- 4. Delete sub-assignments 105..120
DELETE FROM "ScheduleAssignments" WHERE "Id" BETWEEN 105 AND 120;

-- 5. Reset assignment 104 to Accepted
UPDATE "ScheduleAssignments"
SET "Status" = 1, "TerminatedAt" = NULL
WHERE "Id" = 104;

-- 6. Re-attach all remaining JI in order 35 to assignment 104
UPDATE "JobInstances"
SET "ScheduleAssignmentId" = 104,
    "NeedsSubstitute" = false
WHERE "OrderId" = 35
  AND "OrderScheduleId" = 46;

-- 7. Set canonical times 08:15-09:15 (the 1001 row with 09:00-10:00 was a leftover)
UPDATE "JobInstances"
SET "StartTime" = '08:15:00', "EndTime" = '09:15:00'
WHERE "OrderId" = 35;

-- 8. Status: past = Completed (2), today/future = Upcoming (0)
UPDATE "JobInstances"
SET "Status" = 2
WHERE "OrderId" = 35 AND "ScheduledDate" < CURRENT_DATE;

UPDATE "JobInstances"
SET "Status" = 0
WHERE "OrderId" = 35 AND "ScheduledDate" >= CURRENT_DATE;

-- 9. Cosmetic: remove sub-assignment notifications referencing order 35
DELETE FROM "HNotifications"
WHERE "OrderId" = 35
  AND "Type" IN (33, 36, 37);

-- 10. Restore Order 35 to FullAssigned
UPDATE "Orders" SET "Status" = 2 WHERE "Id" = 35;

-- 11. Resync sequences
SELECT setval('"ScheduleAssignments_Id_seq"', GREATEST((SELECT MAX("Id") FROM "ScheduleAssignments"), 1));
SELECT setval('"JobInstances_Id_seq"', GREATEST((SELECT MAX("Id") FROM "JobInstances"), 1));

COMMIT;

-- ─── Verify ───
SELECT 'JobInstances after cleanup' AS section;
SELECT "Id","ScheduleAssignmentId","ScheduledDate","StartTime","EndTime","Status","NeedsSubstitute"
FROM "JobInstances"
WHERE "OrderId" = 35
ORDER BY "ScheduledDate","StartTime","Id";

SELECT 'ScheduleAssignments after cleanup' AS section;
SELECT "Id","StudentId","Status","IsJobInstanceSub","TerminatedAt"
FROM "ScheduleAssignments" WHERE "OrderId"=35 ORDER BY "Id";
