-- Find Petra Novak student
SELECT s."UserId", c."FullName"
FROM "Students" s
JOIN "ContactInfos" c ON c."Id" = s."ContactId"
WHERE c."FullName" ILIKE '%petra%novak%';

-- Look at a healthy order (e.g. order 35) - its schedule + first few job instances
SELECT os."Id", os."DayOfWeek", os."StartTime", os."EndTime" 
FROM "OrderSchedules" os WHERE os."OrderId" = 35;

SELECT ji."Id", ji."OrderId", ji."ScheduledDate", ji."StartTime", ji."EndTime", ji."Status", ji."OrderScheduleId", ji."ScheduleAssignmentId"
FROM "JobInstances" ji WHERE ji."OrderId" = 35 ORDER BY ji."ScheduledDate" LIMIT 5;

SELECT sa."Id", sa."OrderId", sa."StudentId", sa."OrderScheduleId", sa."Status", sa."IsJobInstanceSub"
FROM "ScheduleAssignments" sa WHERE sa."OrderId" = 35 ORDER BY sa."Id" LIMIT 5;

-- Full order 20 info
SELECT * FROM "Orders" WHERE "Id" = 20;
