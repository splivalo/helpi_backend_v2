-- Inspect sessions on 12.05.2026 (Tuesday) and assignments
SELECT 'JobInstances' AS section;
SELECT "Id","OrderId","OrderScheduleId","ScheduleAssignmentId","ScheduledDate","StartTime","EndTime","Status","NeedsSubstitute","PrevAssignmentId","RescheduledToId"
FROM "JobInstances"
WHERE "ScheduledDate" = '2026-05-12'
ORDER BY "Id";

SELECT 'ScheduleAssignments referenced' AS section;
SELECT sa."Id", sa."OrderScheduleId", sa."OrderId", sa."StudentId", sa."Status", sa."IsJobInstanceSub", sa."PrevAssignmentId", sa."AssignedAt", sa."AcceptedAt", sa."TerminatedAt"
FROM "ScheduleAssignments" sa
WHERE sa."Id" IN (
  SELECT DISTINCT "ScheduleAssignmentId" FROM "JobInstances" WHERE "ScheduledDate" = '2026-05-12' AND "ScheduleAssignmentId" IS NOT NULL
  UNION
  SELECT DISTINCT "PrevAssignmentId" FROM "JobInstances" WHERE "ScheduledDate" = '2026-05-12' AND "PrevAssignmentId" IS NOT NULL
)
ORDER BY sa."Id";

SELECT 'All assignments for relevant orders' AS section;
SELECT sa."Id", sa."OrderScheduleId", sa."OrderId", sa."StudentId", sa."Status", sa."IsJobInstanceSub", sa."PrevAssignmentId", sa."AssignedAt", sa."TerminatedAt"
FROM "ScheduleAssignments" sa
WHERE sa."OrderId" IN (
  SELECT DISTINCT "OrderId" FROM "JobInstances" WHERE "ScheduledDate" = '2026-05-12'
)
ORDER BY sa."OrderId", sa."Id";
