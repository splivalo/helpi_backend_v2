BEGIN;

-- 1. Delete all orphaned IsJobInstanceSub assignments for order 35
DELETE FROM "ScheduleAssignments"
WHERE "OrderId" = 35
  AND "IsJobInstanceSub" = true;

-- 2. Reset session 1002 back to clean Upcoming state with Petra (assignment 104)
UPDATE "JobInstances"
SET 
  "Status" = 0,                  -- Upcoming
  "ScheduleAssignmentId" = 104,  -- Petra via shared assignment
  "PrevAssignmentId" = NULL,
  "NeedsSubstitute" = false,
  "RescheduledToId" = NULL,
  "RescheduledAt" = NULL
WHERE "Id" = 1002;

-- 3. Ensure order 35 is FullAssigned
UPDATE "Orders"
SET "Status" = 2
WHERE "Id" = 35;

COMMIT;

-- Verify
SELECT 
  ji."Id",
  ji."ScheduledDate",
  ji."Status" as ji_status,
  ji."ScheduleAssignmentId",
  ji."NeedsSubstitute",
  cnt."FullName" as student_name
FROM "JobInstances" ji
LEFT JOIN "ScheduleAssignments" sa ON sa."Id" = ji."ScheduleAssignmentId"
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE ji."OrderId" = 35
ORDER BY ji."ScheduledDate";
