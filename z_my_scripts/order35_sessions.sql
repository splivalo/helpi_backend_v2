-- See all sessions for order 35 with their assignments and student names
SELECT 
  ji."Id",
  ji."ScheduledDate",
  ji."Status" as ji_status,
  ji."ScheduleAssignmentId",
  sa."IsJobInstanceSub",
  sa."Status" as sa_status,
  sa."StudentId",
  c."FullName" as student_name
FROM "JobInstances" ji
LEFT JOIN "ScheduleAssignments" sa ON sa."Id" = ji."ScheduleAssignmentId"
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" c ON c."Id" = s."ContactId"
WHERE ji."OrderId" = 35
ORDER BY ji."ScheduledDate";
