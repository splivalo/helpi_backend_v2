-- Check order 20 full state
SELECT 
  o."Id" as order_id,
  o."Status" as order_status,
  c."FullName" as senior_name
FROM "Orders" o
LEFT JOIN "Seniors" sr ON sr."Id" = o."SeniorId"
LEFT JOIN "ContactInfos" c ON c."Id" = sr."ContactId"
WHERE o."Id" = 20;

-- All sessions for order 20
SELECT 
  ji."Id",
  ji."ScheduledDate",
  ji."StartTime",
  ji."EndTime",
  ji."Status" as ji_status,
  ji."ScheduleAssignmentId",
  ji."PrevAssignmentId",
  ji."NeedsSubstitute",
  ji."RescheduledToId",
  sa."IsJobInstanceSub",
  sa."Status" as sa_status,
  sa."StudentId",
  cnt."FullName" as student_name
FROM "JobInstances" ji
LEFT JOIN "ScheduleAssignments" sa ON sa."Id" = ji."ScheduleAssignmentId"
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE ji."OrderId" = 20
ORDER BY ji."ScheduledDate";

-- All schedule assignments for order 20
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
WHERE sa."OrderId" = 20
ORDER BY sa."Id";
