-- Full May picture for order 35 (all JIs)
SELECT ji."Id", ji."ScheduledDate", ji."StartTime",
    CASE ji."Status" WHEN 0 THEN 'Upcoming' WHEN 1 THEN 'InProgress' WHEN 2 THEN 'Completed' WHEN 3 THEN 'Cancelled' WHEN 4 THEN 'Rescheduled' END as ji_status,
    ji."ScheduleAssignmentId", ji."NeedsSubstitute", cnt."FullName"
FROM "JobInstances" ji
LEFT JOIN "ScheduleAssignments" sa ON sa."Id" = ji."ScheduleAssignmentId"
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE ji."OrderId" = 35 AND ji."NeedsSubstitute" = false AND ji."Status" != 3
ORDER BY ji."ScheduledDate", ji."StartTime", ji."Id";

-- Active SAs for order 35
SELECT sa."Id", sa."Status", sa."StudentId", sa."IsJobInstanceSub", sa."PrevAssignmentId",
    CASE sa."Status" WHEN 0 THEN 'Pending' WHEN 1 THEN 'Accepted' WHEN 2 THEN 'Declined' WHEN 3 THEN 'Terminated' WHEN 4 THEN 'Completed' END as status_str,
    cnt."FullName"
FROM "ScheduleAssignments" sa
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE sa."OrderId" = 35 AND sa."Status" IN (0, 1, 4)
ORDER BY sa."Id";
