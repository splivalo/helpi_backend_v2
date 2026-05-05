-- Current state of JI 1040 (12.05) and all SAs for order 35
SELECT ji."Id", ji."ScheduledDate", ji."StartTime", ji."Status", ji."ScheduleAssignmentId", ji."NeedsSubstitute"
FROM "JobInstances" ji WHERE ji."Id" = 1040;

-- All SAs for order 35 - current state
SELECT sa."Id", sa."StudentId", sa."Status", sa."IsJobInstanceSub", sa."PrevAssignmentId", cnt."FullName"
FROM "ScheduleAssignments" sa
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE sa."OrderId" = 35 ORDER BY sa."Id";

-- JIs for order 35 in May 2026
SELECT ji."Id", ji."ScheduledDate", ji."StartTime", 
    CASE ji."Status" WHEN 0 THEN 'Upcoming' WHEN 1 THEN 'InProgress' WHEN 2 THEN 'Completed' WHEN 3 THEN 'Cancelled' WHEN 4 THEN 'Rescheduled' END as ji_status,
    ji."ScheduleAssignmentId", ji."NeedsSubstitute",
    cnt."FullName" as student_name
FROM "JobInstances" ji
LEFT JOIN "ScheduleAssignments" sa ON sa."Id" = ji."ScheduleAssignmentId"
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE ji."OrderId" = 35 AND ji."ScheduledDate" BETWEEN '2026-05-05' AND '2026-05-31'
ORDER BY ji."ScheduledDate", ji."Id";
