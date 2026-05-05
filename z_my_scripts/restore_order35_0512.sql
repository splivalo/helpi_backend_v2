-- =====================================================================
-- Restore Order 35 (Tue 08:15-09:15 with Petra/SA 104) to a clean state.
-- Cleans up the broken cascade of sub-assignments + duplicate JobInstances
-- created during the cancel/restore session bug experimentation.
--
-- Final state goal:
--   * ScheduleAssignment 104  → Status=Accepted (1), TerminatedAt=NULL
--   * All job instances on OrderSchedule 46 → ScheduleAssignmentId=104,
--       NeedsSubstitute=false, no orphan stubs
--   * Past sessions (before 2026-05-05) → Completed
--   * Sessions on/after 2026-05-05 → Upcoming
--   * Sub-assignments 105-120 are removed
-- =====================================================================
BEGIN;

-- 1. First null out FK references in JobInstances that point to soon-to-be-deleted sub-assignments
UPDATE "JobInstances"
SET "ScheduleAssignmentId" = NULL,
    "PrevAssignmentId" = NULL,
    "RescheduledToId" = NULL
WHERE "OrderId" = 35
  AND ("ScheduleAssignmentId" IS NULL
       OR "ScheduleAssignmentId" BETWEEN 105 AND 120
       OR "PrevAssignmentId" BETWEEN 104 AND 120);

-- 2. Delete duplicate job instances on 2026-05-12 (we keep one canonical row, prefer 1002)
-- Also delete experimental duplicates on 2026-05-04 / 2026-05-05 that came from sub-assignments.
DELETE FROM "JobInstances"
WHERE "Id" IN (1014, 1020, 1056, 1072, 1023, 1055, 1071);

-- 3. Delete sub-assignments 105..120 (keep 104)
-- First null out FK references in any remaining JI
UPDATE "JobInstances"
SET "ScheduleAssignmentId" = NULL,
    "PrevAssignmentId" = NULL
WHERE "ScheduleAssignmentId" BETWEEN 105 AND 120
   OR "PrevAssignmentId" BETWEEN 105 AND 120;

DELETE FROM "ScheduleAssignments" WHERE "Id" BETWEEN 105 AND 120;

-- 4. Reset assignment 104 to Accepted
UPDATE "ScheduleAssignments"
SET "Status" = 1,            -- Accepted
    "TerminatedAt" = NULL
WHERE "Id" = 104;

-- 5. Re-attach all OrderSchedule 46 job instances to assignment 104, clear NeedsSubstitute
UPDATE "JobInstances"
SET "ScheduleAssignmentId" = 104,
    "NeedsSubstitute" = false,
    "PrevAssignmentId" = NULL,
    "RescheduledToId" = NULL,
    "RescheduledAt" = NULL
WHERE "OrderId" = 35
  AND "OrderScheduleId" = 46;

-- 6. Set status: past = Completed (2), today/future = Upcoming (0)
UPDATE "JobInstances"
SET "Status" = 2  -- Completed
WHERE "OrderId" = 35
  AND "ScheduledDate" < CURRENT_DATE;

UPDATE "JobInstances"
SET "Status" = 0  -- Upcoming
WHERE "OrderId" = 35
  AND "ScheduledDate" >= CURRENT_DATE;

-- 7. Make sure StartTime/EndTime is the canonical 08:15-09:15 (some JI 1001 had 09:00-10:00 leftover)
UPDATE "JobInstances"
SET "StartTime" = '08:15:00',
    "EndTime" = '09:15:00'
WHERE "OrderId" = 35 AND "OrderScheduleId" = 46;

-- 8. Remove notifications referencing deleted sub-assignments (cosmetic)
DELETE FROM "HNotifications"
WHERE "OrderId" = 35
  AND "Type" IN (33, 36, 37);   -- AssignmentPending, AssignmentRevoked, JobReactivated

-- 9. Restore Order 35 status to FullAssigned (2)
UPDATE "Orders" SET "Status" = 2 WHERE "Id" = 35;

-- 10. Reset sequence (no rows added, but safe)
SELECT setval('"ScheduleAssignments_Id_seq"', (SELECT MAX("Id") FROM "ScheduleAssignments"));
SELECT setval('"JobInstances_Id_seq"', (SELECT MAX("Id") FROM "JobInstances"));

COMMIT;

-- Verify
SELECT 'After cleanup' AS section;
SELECT "Id","ScheduleAssignmentId","ScheduledDate","StartTime","EndTime","Status","NeedsSubstitute"
FROM "JobInstances"
WHERE "OrderId" = 35
ORDER BY "ScheduledDate","StartTime","Id";

SELECT "Id","StudentId","Status","IsJobInstanceSub","TerminatedAt"
FROM "ScheduleAssignments" WHERE "OrderId"=35 ORDER BY "Id";
