SELECT "Id","OrderId","DayOfWeek","StartTime","EndTime" FROM "OrderSchedules" WHERE "Id" = 46;

SELECT "Id","OrderNumber","SeniorId","StartDate","EndDate","RecurrencePattern","Status","IsArchived"
FROM "Orders" WHERE "Id"=35;

-- All JI for order 35 in May 2026, ordered by date+time
SELECT "Id","OrderScheduleId","ScheduleAssignmentId","ScheduledDate","StartTime","EndTime","Status","NeedsSubstitute","PrevAssignmentId","RescheduledToId"
FROM "JobInstances"
WHERE "OrderId" = 35 AND "ScheduledDate" BETWEEN '2026-04-01' AND '2026-06-30'
ORDER BY "ScheduledDate","StartTime","Id";

-- All assignments for order 35 (we already saw partial)
SELECT "Id","OrderScheduleId","StudentId","Status","IsJobInstanceSub","PrevAssignmentId","AssignedAt","TerminatedAt"
FROM "ScheduleAssignments" WHERE "OrderId"=35 ORDER BY "Id";
