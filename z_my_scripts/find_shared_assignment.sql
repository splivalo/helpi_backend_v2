-- Find the original shared assignment for order 35, orderScheduleId 46
SELECT 
  sa."Id",
  sa."StudentId",
  sa."OrderId",
  sa."OrderScheduleId",
  sa."Status",
  sa."IsJobInstanceSub"
FROM "ScheduleAssignments" sa
WHERE sa."OrderScheduleId" = 46
  AND sa."IsJobInstanceSub" = false
ORDER BY sa."Id";
