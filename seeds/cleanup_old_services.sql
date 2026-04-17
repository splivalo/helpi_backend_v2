-- ============================================
-- CLEANUP: Remove old v1 services, keep only 6 v2
-- ============================================
-- Run this ONCE on databases with 60 old services.
-- Valid v2 service IDs: 1, 4, 11, 21, 31, 41
--
-- Category mapping (seeder auto-increment):
--   Cat 1 (Aktivnosti) → IDs 1-10  → v2 ID  1 (Društvo)
--   Cat 2 (Kupovina)   → IDs 11-20 → v2 ID 11 (Kupovina)
--   Cat 3 (Kućanstvo)  → IDs 21-30 → v2 ID 21 (Pomoć u kući)
--   Cat 4 (Pratnja)    → IDs 31-40 → v2 ID 31 (Pratnja)
--   Cat 5 (Podrška)    → IDs 41-50 → v2 ID 41 (Ostalo)
--   Cat 6 (Ljubimci)   → IDs 51-60 → v2 ID 41 (Ostalo)
--   Exception: ID 4 is v2 "Šetnja" (walking)
-- ============================================

BEGIN;

-- ─── 1. REMAP OrderService references ───
-- Category 1 (Aktivnosti) → v2 ID 1 (Društvo), except ID 4 → stays 4
UPDATE "OrderServices" SET "ServiceId" = 1
  WHERE "ServiceId" IN (2,3,5,6,7,8,9,10);

-- Category 2 (Kupovina) → v2 ID 11
UPDATE "OrderServices" SET "ServiceId" = 11
  WHERE "ServiceId" IN (12,13,14,15,16,17,18,19,20);

-- Category 3 (Kućanstvo) → v2 ID 21
UPDATE "OrderServices" SET "ServiceId" = 21
  WHERE "ServiceId" IN (22,23,24,25,26,27,28,29,30);

-- Category 4 (Pratnja) → v2 ID 31
UPDATE "OrderServices" SET "ServiceId" = 31
  WHERE "ServiceId" IN (32,33,34,35,36,37,38,39,40);

-- Category 5 (Podrška) → v2 ID 41
UPDATE "OrderServices" SET "ServiceId" = 41
  WHERE "ServiceId" IN (42,43,44,45,46,47,48,49,50);

-- Category 6 (Ljubimci) → v2 ID 41
UPDATE "OrderServices" SET "ServiceId" = 41
  WHERE "ServiceId" IN (51,52,53,54,55,56,57,58,59,60);

-- Remove duplicate OrderService rows after remap (same order + same service)
DELETE FROM "OrderServices" a
  USING "OrderServices" b
  WHERE a.ctid < b.ctid
    AND a."OrderId" = b."OrderId"
    AND a."ServiceId" = b."ServiceId";

-- ─── 2. REMAP StudentService references ───
UPDATE "StudentServices" SET "ServiceId" = 1
  WHERE "ServiceId" IN (2,3,5,6,7,8,9,10);

UPDATE "StudentServices" SET "ServiceId" = 11
  WHERE "ServiceId" IN (12,13,14,15,16,17,18,19,20);

UPDATE "StudentServices" SET "ServiceId" = 21
  WHERE "ServiceId" IN (22,23,24,25,26,27,28,29,30);

UPDATE "StudentServices" SET "ServiceId" = 31
  WHERE "ServiceId" IN (32,33,34,35,36,37,38,39,40);

UPDATE "StudentServices" SET "ServiceId" = 41
  WHERE "ServiceId" IN (42,43,44,45,46,47,48,49,50);

UPDATE "StudentServices" SET "ServiceId" = 41
  WHERE "ServiceId" IN (51,52,53,54,55,56,57,58,59,60);

-- Remove duplicate StudentService rows after remap
DELETE FROM "StudentServices" a
  USING "StudentServices" b
  WHERE a.ctid < b.ctid
    AND a."StudentId" = b."StudentId"
    AND a."ServiceId" = b."ServiceId";

-- ─── 3. REMOVE old FK references ───
DELETE FROM "ServiceRegions" WHERE "ServiceId" NOT IN (1,4,11,21,31,41);

-- ─── 4. DELETE old services ───
DELETE FROM "Services" WHERE "Id" NOT IN (1,4,11,21,31,41);

-- ─── 5. UPDATE 6 valid services with correct v2 names ───
UPDATE "Services" SET "Translations" = '{"en": {"Name": "Companionship", "Description": null}, "hr": {"Name": "Društvo", "Description": null}}'::jsonb WHERE "Id" = 1;
UPDATE "Services" SET "Translations" = '{"en": {"Name": "Walking", "Description": null}, "hr": {"Name": "Šetnja", "Description": null}}'::jsonb WHERE "Id" = 4;
UPDATE "Services" SET "Translations" = '{"en": {"Name": "Shopping", "Description": null}, "hr": {"Name": "Kupovina", "Description": null}}'::jsonb WHERE "Id" = 11;
UPDATE "Services" SET "Translations" = '{"en": {"Name": "House Help", "Description": null}, "hr": {"Name": "Pomoć u kući", "Description": null}}'::jsonb WHERE "Id" = 21;
UPDATE "Services" SET "Translations" = '{"en": {"Name": "Escort", "Description": null}, "hr": {"Name": "Pratnja", "Description": null}}'::jsonb WHERE "Id" = 31;
UPDATE "Services" SET "Translations" = '{"en": {"Name": "Other", "Description": null}, "hr": {"Name": "Ostalo", "Description": null}}'::jsonb WHERE "Id" = 41;

-- ─── 6. CLEANUP empty categories ───
DELETE FROM "ServiceCategories"
  WHERE "Id" NOT IN (SELECT DISTINCT "CategoryId" FROM "Services");

COMMIT;

-- ─── VERIFY ───
SELECT "Id", "CategoryId", "Translations"::text FROM "Services" ORDER BY "Id";
SELECT COUNT(*) AS remaining_order_services FROM "OrderServices";
SELECT COUNT(*) AS remaining_student_services FROM "StudentServices";
