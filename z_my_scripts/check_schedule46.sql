-- Check what references session 1002
SELECT 'Reviews' as tbl, COUNT(*) FROM "Reviews" WHERE "JobInstanceId" = 1002
UNION ALL
SELECT 'ScheduleAssignments_via_JobInstances', COUNT(*) FROM "ScheduleAssignments" sa
  JOIN "JobInstances" ji ON ji."ScheduleAssignmentId" = sa."Id" WHERE ji."Id" = 1002;

-- Check ALL JobInstances for schedule 46 (OrderScheduleId) that are upcoming/future
SELECT 
  ji."Id",
  ji."ScheduledDate",
  ji."Status",
  ji."ScheduleAssignmentId",
  ji."NeedsSubstitute"
FROM "JobInstances" ji
WHERE ji."OrderScheduleId" = 46
ORDER BY ji."ScheduledDate";

-- Check all ScheduleAssignments for schedule 46
SELECT 
  sa."Id", sa."StudentId", sa."Status", sa."IsJobInstanceSub"
FROM "ScheduleAssignments" sa
WHERE sa."OrderScheduleId" = 46
ORDER BY sa."Id";
