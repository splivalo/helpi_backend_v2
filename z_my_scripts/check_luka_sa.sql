-- Check which JIs are linked to SA 106, 115, 116
SELECT ji."Id", ji."ScheduledDate", ji."StartTime",
    CASE ji."Status" WHEN 0 THEN 'Upcoming' WHEN 1 THEN 'InProgress' WHEN 2 THEN 'Completed' WHEN 3 THEN 'Cancelled' WHEN 4 THEN 'Rescheduled' END as ji_status,
    ji."ScheduleAssignmentId", cnt."FullName"
FROM "JobInstances" ji
LEFT JOIN "ScheduleAssignments" sa ON sa."Id" = ji."ScheduleAssignmentId"
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE ji."ScheduleAssignmentId" IN (106, 115, 116)
ORDER BY ji."ScheduledDate", ji."Id";

-- Also check what SA 106 JIs look like specifically (Luka in the cancelled reassignment)
SELECT sa."Id", sa."Status", sa."StudentId", sa."IsJobInstanceSub", sa."PrevAssignmentId", cnt."FullName"
FROM "ScheduleAssignments" sa
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE sa."Id" IN (106, 115, 116);
