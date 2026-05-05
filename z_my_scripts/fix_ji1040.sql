BEGIN;

-- Fix JI 1040: link it to SA 118 (Petra, PendingAcceptance)
UPDATE "JobInstances"
SET "ScheduleAssignmentId" = 118
WHERE "Id" = 1040 AND "Status" = 0;

-- Verify
SELECT ji."Id", ji."ScheduledDate", ji."Status", ji."ScheduleAssignmentId", cnt."FullName"
FROM "JobInstances" ji
LEFT JOIN "ScheduleAssignments" sa ON sa."Id" = ji."ScheduleAssignmentId"
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE ji."Id" = 1040;

SELECT sa."Id", sa."Status", sa."StudentId", cnt."FullName"
FROM "ScheduleAssignments" sa
LEFT JOIN "Students" s ON s."UserId" = sa."StudentId"
LEFT JOIN "ContactInfos" cnt ON cnt."Id" = s."ContactId"
WHERE sa."Id" = 118;

COMMIT;
