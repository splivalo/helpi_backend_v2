-- Check by SeniorId (senior Stipe Splivalo = SeniorId 8 from order 20)
SELECT ji."Id", ji."OrderId", ji."SeniorId", ji."ScheduledDate", ji."StartTime", ji."Status"
FROM "JobInstances" ji
WHERE ji."SeniorId" = 8
ORDER BY ji."ScheduledDate";

-- Check ALL orders for SeniorId=8
SELECT o."Id", o."OrderNumber", o."Status", o."StartDate", o."CancelledAt"
FROM "Orders" o
WHERE o."SeniorId" = 8;
