-- Check job instance 1002 and related assignments
SELECT 
  ji."Id", 
  ji."ScheduledDate", 
  ji."StartTime", 
  ji."Status",
  ji."ScheduleAssignmentId",
  ji."PrevAssignmentId",
  ji."NeedsSubstitute",
  ji."RescheduledToId"
FROM "JobInstances" ji 
WHERE ji."Id" = 1002;

-- Check all schedule assignments linked to this session
SELECT 
  sa."Id",
  sa."StudentId",
  sa."OrderId",
  sa."Status",
  sa."IsJobInstanceSub",
  sa."PrevAssignmentId",
  sa."AssignedAt"
FROM "ScheduleAssignments" sa
WHERE sa."Id" IN (
  SELECT ji."ScheduleAssignmentId" FROM "JobInstances" ji WHERE ji."Id" = 1002
  UNION
  SELECT ji."PrevAssignmentId" FROM "JobInstances" ji WHERE ji."Id" = 1002
)
OR sa."OrderId" = (SELECT ji."OrderId" FROM "JobInstances" ji WHERE ji."Id" = 1002)
   AND sa."IsJobInstanceSub" = true
ORDER BY sa."Id";

-- Check what order and who is Petra
SELECT 
  ji."OrderId",
  ji."SeniorId",
  ji."OrderScheduleId"
FROM "JobInstances" ji 
WHERE ji."Id" = 1002;
