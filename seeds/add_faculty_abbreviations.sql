-- Add Abbreviation field to Faculty translations JSON
-- Safe to re-run: overwrites Abbreviation each time
-- Does NOT affect admin app (admin only uses Name)

-- 1  Dramska akademija
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"DA"'), '{en,Abbreviation}', '"DA"') WHERE "Id" = 1;
-- 2  Likovna akademija
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"LA"'), '{en,Abbreviation}', '"LA"') WHERE "Id" = 2;
-- 3  Agronomski fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"AGR"'), '{en,Abbreviation}', '"AGR"') WHERE "Id" = 3;
-- 4  Arhitektonski fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"AF"'), '{en,Abbreviation}', '"AF"') WHERE "Id" = 4;
-- 5  Edukacijsko rehabilitacijski fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"ERF"'), '{en,Abbreviation}', '"ERF"') WHERE "Id" = 5;
-- 6  Ekonomski fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"EFZG"'), '{en,Abbreviation}', '"EFZG"') WHERE "Id" = 6;
-- 7  Elektrotehnika i računarstvo
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"FER"'), '{en,Abbreviation}', '"FER"') WHERE "Id" = 7;
-- 8  Filozofija i religijske znanosti
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"FFRZ"'), '{en,Abbreviation}', '"FFRZ"') WHERE "Id" = 8;
-- 9  Hrvatski studiji
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"FHS"'), '{en,Abbreviation}', '"FHS"') WHERE "Id" = 9;
-- 10 Kemijsko inženjerstvo i tehnologija
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"FKIT"'), '{en,Abbreviation}', '"FKIT"') WHERE "Id" = 10;
-- 11 Organizacija i informatika
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"FOI"'), '{en,Abbreviation}', '"FOI"') WHERE "Id" = 11;
-- 12 Političke znanosti
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"FPZG"'), '{en,Abbreviation}', '"FPZG"') WHERE "Id" = 12;
-- 13 Prometne znanosti
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"FPZ"'), '{en,Abbreviation}', '"FPZ"') WHERE "Id" = 13;
-- 14 Strojarstvo i brodogradnja
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"FSB"'), '{en,Abbreviation}', '"FSB"') WHERE "Id" = 14;
-- 15 Šumarstvo
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"FŠDT"'), '{en,Abbreviation}', '"FWT"') WHERE "Id" = 15;
-- 16 Farmacija i biokemija
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"FBF"'), '{en,Abbreviation}', '"FBF"') WHERE "Id" = 16;
-- 17 Filozofski fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"FFZG"'), '{en,Abbreviation}', '"FFZG"') WHERE "Id" = 17;
-- 18 Geodetski fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"GEOF"'), '{en,Abbreviation}', '"GEOF"') WHERE "Id" = 18;
-- 19 Geotehnički fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"GEOTEH"'), '{en,Abbreviation}', '"GEOTEH"') WHERE "Id" = 19;
-- 20 Građevinski fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"GF"'), '{en,Abbreviation}', '"GF"') WHERE "Id" = 20;
-- 21 Grafički fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"GRF"'), '{en,Abbreviation}', '"GRF"') WHERE "Id" = 21;
-- 22 Katolički bogoslovni fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"KBF"'), '{en,Abbreviation}', '"KBF"') WHERE "Id" = 22;
-- 23 Kineziološki fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"KIF"'), '{en,Abbreviation}', '"KIF"') WHERE "Id" = 23;
-- 24 Medicinski fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"MEF"'), '{en,Abbreviation}', '"MEF"') WHERE "Id" = 24;
-- 25 Metalurški fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"MET"'), '{en,Abbreviation}', '"MET"') WHERE "Id" = 25;
-- 26 Glazbena akademija
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"MUZA"'), '{en,Abbreviation}', '"MUZA"') WHERE "Id" = 26;
-- 27 Pravni fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"PRAVO"'), '{en,Abbreviation}', '"PRAVO"') WHERE "Id" = 27;
-- 28 Prehrambeno biotehnološki fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"PBF"'), '{en,Abbreviation}', '"PBF"') WHERE "Id" = 28;
-- 29 Prirodne znanosti (PMF)
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"PMF"'), '{en,Abbreviation}', '"PMF"') WHERE "Id" = 29;
-- 30 Rudarstvo, geologija i nafta
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"RGN"'), '{en,Abbreviation}', '"RGN"') WHERE "Id" = 30;
-- 31 Stomatološki fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"SFZG"'), '{en,Abbreviation}', '"SFZG"') WHERE "Id" = 31;
-- 32 Tekstilno tehnološki fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"TTF"'), '{en,Abbreviation}', '"TTF"') WHERE "Id" = 32;
-- 33 Učiteljski fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"UFZG"'), '{en,Abbreviation}', '"UFZG"') WHERE "Id" = 33;
-- 34 Veterinarski fakultet
UPDATE "Faculties" SET "Translations" = jsonb_set(jsonb_set("Translations"::jsonb, '{hr,Abbreviation}', '"VEF"'), '{en,Abbreviation}', '"VEF"') WHERE "Id" = 34;
