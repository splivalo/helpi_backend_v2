-- Check order 35 full state
SELECT o."Id", o."Status", c."FullName" as senior_name
FROM "Orders" o
LEFT JOIN "Seniors" sr ON sr."Id" = o."SeniorId"
LEFT JOIN "ContactInfos" c ON c."Id" = sr."ContactId"
WHERE o."Id" = 35;

-- All sessions
SELECT 
  ji."Id",
  ji."ScheduledDate",
  ji."StartTime",
  ji."Status" as ji_status,
  ji."ScheduleAssignmentId",
  ji."PrevAssignmentId",
  ji."NeedsSubstitute",
  sa."IsJobInstanceSub",
  sa."Status" as sa_status,
  sa."StudentId",
  cnt."FullName" as student_name
FROM "JobInstances" ji
LEFT JOIN "ScheduleAssignments" sa ON sa."Id" = ji."ScheduleAssignmentId"
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE ji."OrderId" = 35
ORDER BY ji."ScheduledDate";

-- All assignments for order 35
SELECT 
  sa."Id",
  sa."StudentId",
  sa."OrderScheduleId",
  sa."Status",
  sa."IsJobInstanceSub",
  sa."PrevAssignmentId",
  cnt."FullName" as student_name
FROM "ScheduleAssignments" sa
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE sa."OrderId" = 35
ORDER BY sa."Id";
