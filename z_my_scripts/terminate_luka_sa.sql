UPDATE "ScheduleAssignments" SET "Status" = 3, "TerminationReason" = 6, "TerminatedAt" = NOW() WHERE "Id" IN (106, 115, 116) AND "Status" = 1;
