-- Check SA 117 and 118 current state
SELECT sa."Id", sa."Status", sa."StudentId", sa."IsJobInstanceSub", sa."PrevAssignmentId"
FROM "ScheduleAssignments" sa
WHERE sa."Id" IN (117, 118);

-- Also check if backend recently ran MaintainOrderStatuses and changed things
-- Check all SAs for order 35
SELECT sa."Id", sa."Status", sa."StudentId", sa."IsJobInstanceSub"
FROM "ScheduleAssignments" sa
WHERE sa."OrderId" = 35
ORDER BY sa."Id";
