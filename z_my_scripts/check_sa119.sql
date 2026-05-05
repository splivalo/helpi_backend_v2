-- What JI is linked to SA 119?
SELECT ji."Id", ji."ScheduledDate", ji."StartTime", ji."Status", ji."ScheduleAssignmentId"
FROM "JobInstances" ji
WHERE ji."ScheduleAssignmentId" = 119 OR ji."Id" = 1040;

-- Full SA 119 details
SELECT sa."Id", sa."Status", sa."StudentId", sa."IsJobInstanceSub", sa."PrevAssignmentId", sa."OrderScheduleId", cnt."FullName"
FROM "ScheduleAssignments" sa
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE sa."Id" IN (118, 119);

-- Sessions in May for order 35
SELECT ji."Id", ji."ScheduledDate", ji."StartTime", 
    CASE ji."Status" WHEN 0 THEN 'Upcoming' WHEN 1 THEN 'InProgress' WHEN 2 THEN 'Completed' WHEN 3 THEN 'Cancelled' WHEN 4 THEN 'Rescheduled' END as ji_status,
    ji."ScheduleAssignmentId", ji."NeedsSubstitute", cnt."FullName"
FROM "JobInstances" ji
LEFT JOIN "ScheduleAssignments" sa ON sa."Id" = ji."ScheduleAssignmentId"
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE ji."OrderId" = 35 AND ji."ScheduledDate" BETWEEN '2026-05-05' AND '2026-05-31'
ORDER BY ji."ScheduledDate", ji."Id";
