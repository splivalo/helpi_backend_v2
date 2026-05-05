-- ============================================================
-- FIX ROUND 2: Order 35 (OrderNumber=20) - Stipe Splivalo (OSD)
-- State after user's cancel + failed restore + "Promijeni studenta"
-- 
-- Goals:
--   - SA 119 (Petra, Accepted) = sole active assignment
--   - One JI per Tuesday from 12.05 onwards (SA=119 series)
--   - JI 1040 (SA=118 Declined) = delete (JI 1056 covers 12.05)
--   - JI 1041 (SA=null, 19.05) = delete (JI 1057 covers 19.05)
--   - JI 1055 (SA=119, 05.05 Upcoming) = cancel (today already done: JI 1023 Completed)
--   - JIs 1042-1052 (SA=117, duplicates) = delete
--   - SA 117 = Terminate (superseded by SA 119)
-- ============================================================

BEGIN;

-- Step 1: Check for CouponUsages / JobRequests on JIs to be deleted
-- (Adding safety check inline)
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM "CouponUsages" WHERE "JobInstanceId" IN (1040,1041,1042,1043,1044,1045,1046,1047,1048,1049,1050,1051,1052)) THEN
    RAISE EXCEPTION 'CouponUsages exist for JIs being deleted - handle manually';
  END IF;
  IF EXISTS (SELECT 1 FROM "JobRequests" WHERE "JobInstanceId" IN (1040,1041,1042,1043,1044,1045,1046,1047,1048,1049,1050,1051,1052)) THEN
    RAISE EXCEPTION 'JobRequests exist for JIs being deleted - handle manually';
  END IF;
END $$;

-- Step 2: Delete JI 1040 (SA=118/Declined, 12.05) - covered by JI 1056 (SA=119)
DELETE FROM "JobInstances" WHERE "Id" = 1040;

-- Step 3: Delete JI 1041 (SA=null, 19.05 Upcoming) - covered by JI 1057 (SA=119)
DELETE FROM "JobInstances" WHERE "Id" = 1041;

-- Step 4: Delete SA=117 duplicate upcoming JIs (1042-1052)
DELETE FROM "JobInstances" WHERE "Id" IN (1042,1043,1044,1045,1046,1047,1048,1049,1050,1051,1052);

-- Step 5: Cancel JI 1055 (SA=119, 05.05 Upcoming - today already completed via JI 1023)
UPDATE "JobInstances" SET "Status" = 3 WHERE "Id" = 1055;

-- Step 6: Terminate SA 117 (superseded by SA 119)
UPDATE "ScheduleAssignments"
SET "Status" = 3, "TerminationReason" = 6, "TerminatedAt" = NOW()
WHERE "Id" = 117 AND "Status" = 1;

COMMIT;

-- ============================================================
-- VERIFICATION
-- ============================================================
SELECT '=== Active & upcoming JIs for order 35 from 2026-05-05 ===' as info;
SELECT 
    ji."Id", ji."ScheduledDate", ji."StartTime",
    CASE ji."Status" WHEN 0 THEN 'Upcoming' WHEN 1 THEN 'InProgress' WHEN 2 THEN 'Completed' WHEN 3 THEN 'Cancelled' WHEN 4 THEN 'Rescheduled' END as ji_status,
    ji."ScheduleAssignmentId", ji."NeedsSubstitute", cnt."FullName"
FROM "JobInstances" ji
LEFT JOIN "ScheduleAssignments" sa ON sa."Id" = ji."ScheduleAssignmentId"
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE ji."OrderId" = 35 AND ji."NeedsSubstitute" = false AND ji."ScheduledDate" >= '2026-05-05'
ORDER BY ji."ScheduledDate", ji."Id";

SELECT '=== All SAs for order 35 ===' as info;
SELECT sa."Id",
    CASE sa."Status" WHEN 0 THEN 'Pending' WHEN 1 THEN 'Accepted' WHEN 2 THEN 'Declined' WHEN 3 THEN 'Terminated' WHEN 4 THEN 'Completed' END as status_str,
    sa."IsJobInstanceSub", sa."StudentId", cnt."FullName"
FROM "ScheduleAssignments" sa
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE sa."OrderId" = 35
ORDER BY sa."Id";
