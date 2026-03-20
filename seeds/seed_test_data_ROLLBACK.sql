-- ============================================================
-- ROLLBACK: Deletes data added by seed_test_data.sql
-- Only removes: Reviews, PromoCodes, HNotifications
-- Does NOT touch: ScheduleAssignments, JobInstances,
-- StudentContracts, StudentAvailabilitySlots (managed by seed_mock_data.sql)
-- ============================================================

BEGIN;

-- Delete in reverse FK order
DELETE FROM "HNotifications" WHERE "Id" BETWEEN 1 AND 14;
DELETE FROM "PromoCodes" WHERE "Id" BETWEEN 1 AND 4;
DELETE FROM "Reviews" WHERE "Id" BETWEEN 1 AND 8;

-- Reset sequences
SELECT setval('"Reviews_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "Reviews"));
SELECT setval('"PromoCodes_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "PromoCodes"));
SELECT setval('"HNotifications_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "HNotifications"));

COMMIT;

-- Verification:
SELECT 'Reviews' AS tbl, COUNT(*) FROM "Reviews"
UNION ALL SELECT 'PromoCodes', COUNT(*) FROM "PromoCodes"
UNION ALL SELECT 'HNotifications', COUNT(*) FROM "HNotifications";
