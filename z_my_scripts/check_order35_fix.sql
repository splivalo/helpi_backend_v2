-- Full row for a clean completed JI from order 35
SELECT * FROM "JobInstances" WHERE "Id" IN (1023, 1039, 1040, 1024) ORDER BY "Id";

-- Check all upcoming JIs for order 35 (Status=0)
SELECT ji."Id", ji."ScheduledDate", ji."StartTime", ji."ScheduleAssignmentId", ji."OrderScheduleId", ji."NeedsSubstitute"
FROM "JobInstances" ji WHERE ji."OrderId" = 35 AND ji."Status" = 0 ORDER BY ji."ScheduledDate", ji."Id";

-- Check all Cancelled JIs for order 35 (Status=3) from 2026-05-12 onwards
SELECT ji."Id", ji."ScheduledDate", ji."StartTime", ji."Status", ji."ScheduleAssignmentId", ji."NeedsSubstitute"
FROM "JobInstances" ji WHERE ji."OrderId" = 35 AND ji."Status" = 3 AND ji."ScheduledDate" >= '2026-05-12' ORDER BY ji."ScheduledDate", ji."Id";
