-- ============================================
-- CLEANUP: Remove old services from v1, keep only 6 valid
-- ============================================
-- Run this ONCE on databases that still have 60 old services.
-- Valid service IDs: 1, 4, 11, 21, 31, 41
-- ============================================

BEGIN;

-- Remove FK references first
DELETE FROM "ServiceRegions" WHERE "ServiceId" NOT IN (1,4,11,21,31,41);

-- Delete old services
DELETE FROM "Services" WHERE "Id" NOT IN (1,4,11,21,31,41);

-- Update 6 valid services with correct names
UPDATE "Services" SET "Translations" = '{"en": {"Name": "Companionship", "Description": null}, "hr": {"Name": "Društvo", "Description": null}}'::jsonb WHERE "Id" = 1;
UPDATE "Services" SET "Translations" = '{"en": {"Name": "Walking", "Description": null}, "hr": {"Name": "Šetnja", "Description": null}}'::jsonb WHERE "Id" = 4;
UPDATE "Services" SET "Translations" = '{"en": {"Name": "Shopping", "Description": null}, "hr": {"Name": "Kupovina", "Description": null}}'::jsonb WHERE "Id" = 11;
UPDATE "Services" SET "Translations" = '{"en": {"Name": "House Help", "Description": null}, "hr": {"Name": "Kućanstvo", "Description": null}}'::jsonb WHERE "Id" = 21;
UPDATE "Services" SET "Translations" = '{"en": {"Name": "Escort", "Description": null}, "hr": {"Name": "Pratnja", "Description": null}}'::jsonb WHERE "Id" = 31;
UPDATE "Services" SET "Translations" = '{"en": {"Name": "Other", "Description": null}, "hr": {"Name": "Ostalo", "Description": null}}'::jsonb WHERE "Id" = 41;

-- Remove unused category 6 (Pets) if no services reference it
DELETE FROM "ServiceCategories" WHERE "Id" = 6
  AND NOT EXISTS (SELECT 1 FROM "Services" WHERE "CategoryId" = 6);

COMMIT;

-- Verify
SELECT "Id", "CategoryId", "Translations"::text FROM "Services" ORDER BY "Id";
SELECT COUNT(*) AS categories FROM "ServiceCategories";
