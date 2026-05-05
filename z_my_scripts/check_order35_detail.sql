-- Full details for the problematic job instances in order 35 (OrderNumber=20)
SELECT 
  ji."Id", ji."ScheduledDate", ji."StartTime", ji."EndTime", ji."Status",
  ji."ScheduleAssignmentId", ji."PrevAssignmentId", ji."NeedsSubstitute",
  ji."RescheduledToId", ji."RescheduledFromId",
  sa."Id" as sa_id, sa."StudentId", sa."Status" as sa_status, sa."IsJobInstanceSub",
  cnt."FullName" as student_name
FROM "JobInstances" ji
LEFT JOIN "ScheduleAssignments" sa ON sa."Id" = ji."ScheduleAssignmentId"
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE ji."OrderId" = 35 AND ji."ScheduledDate" IN ('2026-05-05','2026-05-12')
ORDER BY ji."ScheduledDate", ji."Id";

-- All ScheduleAssignments for order 35
SELECT 
  sa."Id", sa."StudentId", sa."OrderScheduleId", sa."Status", sa."IsJobInstanceSub",
  sa."PrevAssignmentId", cnt."FullName"
FROM "ScheduleAssignments" sa
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE sa."OrderId" = 35
ORDER BY sa."Id";

-- OrderSchedule for order 35
SELECT * FROM "OrderSchedules" WHERE "OrderId" = 35;
