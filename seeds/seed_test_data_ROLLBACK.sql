-- ╔════════════════════════════════════════════════════════════════╗
-- ║  ROLLBACK: Brise SVE testne podatke iz seed_test_data.sql    ║
-- ║  Pokreni ovo nakon sto zavrsis testiranje!                   ║
-- ╚════════════════════════════════════════════════════════════════╝

BEGIN;

-- Obrnutim redoslijedom brisanja (FK constraints)
DELETE FROM "StudentAvailabilitySlots" WHERE "StudentId" IN (101, 102, 103, 104);
DELETE FROM "HNotifications" WHERE "Id" BETWEEN 1 AND 19;
DELETE FROM "PromoCodes" WHERE "Id" BETWEEN 1 AND 4;
DELETE FROM "Reviews" WHERE "Id" BETWEEN 1 AND 6;
DELETE FROM "JobInstances" WHERE "Id" BETWEEN 1 AND 12;
DELETE FROM "ScheduleAssignments" WHERE "Id" BETWEEN 1 AND 12;
DELETE FROM "StudentContracts" WHERE "Id" BETWEEN 1 AND 4;

-- Vrati studente na Inactive
UPDATE "Students" SET "Status" = 0 WHERE "UserId" IN (101, 102, 103, 104);

-- Orders vec ostaju Pending, ništa za rollback

-- Reset sequences
SELECT setval('"StudentContracts_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "StudentContracts"));
SELECT setval('"ScheduleAssignments_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "ScheduleAssignments"));
SELECT setval('"JobInstances_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "JobInstances"));
SELECT setval('"Reviews_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "Reviews"));
SELECT setval('"PromoCodes_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "PromoCodes"));
SELECT setval('"HNotifications_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "HNotifications"));
-- StudentAvailabilitySlots nema Id sequence (composite PK)

COMMIT;

-- Provjera:
SELECT 'StudentContracts' AS tbl, COUNT(*) FROM "StudentContracts"
UNION ALL SELECT 'ScheduleAssignments', COUNT(*) FROM "ScheduleAssignments"
UNION ALL SELECT 'JobInstances', COUNT(*) FROM "JobInstances"
UNION ALL SELECT 'Reviews', COUNT(*) FROM "Reviews"
UNION ALL SELECT 'PromoCodes', COUNT(*) FROM "PromoCodes"
UNION ALL SELECT 'HNotifications', COUNT(*) FROM "HNotifications"
UNION ALL SELECT 'StudentAvailabilitySlots', COUNT(*) FROM "StudentAvailabilitySlots";
