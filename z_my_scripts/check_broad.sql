-- Check all databases
SELECT datname FROM pg_database WHERE datistemplate = false;

-- Check if any JobInstances exist at all for date 2026-05-05
SELECT ji."Id", ji."OrderId", ji."ScheduledDate", ji."StartTime", ji."Status"
FROM "JobInstances" ji
WHERE ji."ScheduledDate" = '2026-05-05'
LIMIT 20;

-- Check order 20 OrderSchedules
SELECT os."Id", os."DayOfWeek", os."StartTime", os."EndTime"
FROM "OrderSchedules" os
WHERE os."OrderId" = 20;

-- Total count of JobInstances
SELECT COUNT(*) FROM "JobInstances";
