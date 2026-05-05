-- Full info for order 20
SELECT 
  o."Id", o."Status", o."StartDate", o."ServiceId"
FROM "Orders" o
WHERE o."Id" = 20;

-- Order schedules
SELECT 
  os."Id" as os_id, os."DayOfWeek", os."StartTime", os."EndTime", os."DurationMinutes"
FROM "OrderSchedules" os
WHERE os."OrderId" = 20;

-- Services reference
SELECT "Id", "Name" FROM "Services" WHERE "Id" IN (SELECT "ServiceId" FROM "Orders" WHERE "Id" = 20);
