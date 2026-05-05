-- ============================================================
-- FIX: Order 35 (OrderNumber=20) - Stipe Splivalo (OSD)
-- Problem: duplicate JIs on 05.05 + cancelled JI on 12.05 without student
--          caused by SA 104, SA 111, SA 117 all being Accepted simultaneously
-- Solution: delete duplicates, terminate superseded SAs, keep only SA 117 active
-- Date: 2026-05-05
-- ============================================================

BEGIN;
-- Step 0: Delete CouponUsage (Id=4) linked to duplicate JI 1039 (RESTRICT FK)
--   Reviews for JI 1039 (Ids 51,52) are CASCADE so auto-deleted
DELETE FROM "CouponUsages" WHERE "Id" = 4;
-- Step 1: Delete duplicate COMPLETED JI on 05.05.2026
--   Keep JI 1023 (SA=111), delete JI 1039 (SA=117 duplicate)
--   Note: Hangfire jobs 758/759/760 linked to 1039 have already run (session is Completed)
DELETE FROM "JobInstances" WHERE "Id" = 1039;

-- Step 2: Delete cancelled JI on 12.05.2026 with no student
--   This is the one showing as "Vrati termin" in the admin (Cancelled, no SA, PrevAssignment=111)
DELETE FROM "JobInstances" WHERE "Id" = 1024;

-- Step 3: Delete all UPCOMING (Status=0) JIs for order 35 linked to SA 104 or SA 111
--   These are duplicates — SA 117-linked JIs already exist for the same dates
--   SA 104-linked upcoming: 1003,1004,1005,1006,1007,1008,1009,1010,1011,1012,1013
--   SA 111-linked upcoming: 1025,1026,1027,1028,1029,1030,1031,1032,1033,1034,1035,1036
DELETE FROM "JobInstances" WHERE "Id" IN (
    1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011, 1012, 1013,
    1025, 1026, 1027, 1028, 1029, 1030, 1031, 1032, 1033, 1034, 1035, 1036
);

-- Step 4: Terminate the superseded SAs in the Petra chain (107, 110, 111)
--   SA 117 (Petra Novak, PrevAssignment=111) remains as the sole active sub
--   SA 104 (original, non-sub) is also terminated to prevent scheduler regenerating duplicates
UPDATE "ScheduleAssignments"
SET "Status" = 3,             -- Terminated
    "TerminationReason" = 6,  -- AdminIntervention
    "TerminatedAt" = NOW()
WHERE "Id" IN (104, 107, 110, 111)
  AND "Status" = 1;           -- Only update Accepted ones

COMMIT;

-- ============================================================
-- VERIFICATION - check the resulting state
-- ============================================================

SELECT '=== JIs for order 35 from 2026-05-05 ===' as info;
SELECT
    ji."Id",
    ji."ScheduledDate",
    ji."StartTime",
    ji."EndTime",
    CASE ji."Status"
        WHEN 0 THEN 'Upcoming'
        WHEN 1 THEN 'InProgress'
        WHEN 2 THEN 'Completed'
        WHEN 3 THEN 'Cancelled'
        WHEN 4 THEN 'Rescheduled'
    END AS ji_status,
    ji."ScheduleAssignmentId",
    ji."NeedsSubstitute",
    cnt."FullName" as student_name
FROM "JobInstances" ji
LEFT JOIN "ScheduleAssignments" sa ON sa."Id" = ji."ScheduleAssignmentId"
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE ji."OrderId" = 35
  AND ji."ScheduledDate" >= '2026-05-05'
ORDER BY ji."ScheduledDate", ji."StartTime";

SELECT '=== Active ScheduleAssignments for order 35 ===' as info;
SELECT
    sa."Id",
    sa."StudentId",
    CASE sa."Status"
        WHEN 0 THEN 'PendingAcceptance'
        WHEN 1 THEN 'Accepted'
        WHEN 2 THEN 'Declined'
        WHEN 3 THEN 'Terminated'
        WHEN 4 THEN 'Completed'
    END AS sa_status,
    sa."IsJobInstanceSub",
    sa."PrevAssignmentId",
    cnt."FullName" as student_name
FROM "ScheduleAssignments" sa
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE sa."OrderId" = 35
ORDER BY sa."Id";
