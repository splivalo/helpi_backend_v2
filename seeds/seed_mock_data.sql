-- ============================================
-- HELPI MOCK DATA SEED SCRIPT (v2 - realistic)
-- ============================================
-- Mixed student statuses, mixed order statuses,
-- partial availability overlaps, some unassignable orders.
-- ============================================
-- STUDENT STATUS MATRIX:
--   101 Luka  = Active (1)       — Mon/Wed/Fri 08-14 + Thu 13-17, Services: 1,4
--   102 Ana   = Active (1)       — Mon 12-15, Tue/Thu 09-15, Sat 10-14, Services: 1,4,11,21
--   103 Ivan  = Active (1)       — Mon/Wed/Fri 10-17 + Tue/Thu 12-17, Services: 4,31,41
--   104 Petra = AboutToExpire(2) — Mon-Fri 08-18, Services: 1,4,11,31
--   105 Marko = InActive (0)     — no availability, no services
--   106 Maja  = Deactivated (4)  — no availability
--   107 Dino  = Expired (3)      — Tue/Thu 09-13, Services: 21,41 (but expired!)
--
-- ORDER STATUS MATRIX:
--   2, 12-15 = Pending (5 orders)
--   5-9     = FullAssigned (5 orders, with ScheduleAssignment rows + JobInstances)
--   1, 3, 4, 10 = Completed (4 orders, with completed assignments + JobInstances)
--   11       = Cancelled (1 order)
--
-- DIFFERENTTIMES TEST SCENARIOS (frontend _classifyStudent):
--   Order 12 (Mon+Thu 09-12, walking): Petra=full, Luka=differentTimes(Thu 13-17≠09:00)
--   Order 13 (Mon+Thu 11-13, walking): Petra=full, Luka=differentTimes(Thu), Ivan=differentTimes(Thu 12-17≠11:00)
--   Order 14 (Tue+Sat 09-12, companion): Ana=differentTimes(Sat 10-14≠09:00), Petra=unavailable(no Sat)
--   Order 15 (Mon+Tue+Thu, companion): Luka=differentTimes(Mon only green, Tue no slot, Thu wrong time→alt slots)
-- ============================================

BEGIN;

-- ============================================
-- 0. ADMIN USER (info@helpi.social / H3lp!5y5t3m5)
-- ============================================
INSERT INTO "AspNetUsers" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount", "UserType", "CreatedAt", "UpdatedAt", "IsSuspended")
VALUES
(1, 'info@helpi.social', 'INFO@HELPI.SOCIAL', 'info@helpi.social', 'INFO@HELPI.SOCIAL', true, 'AQAAAAIAAYagAAAAELi1OW97jKFjzNAJPxkkJcDzVCiuq65wVchqFhNIPfSCQZ1wOdI60WBmmIFllcaWwA==', 'R3GOCBGRCUB4RY7QBXNC3WFLFLV534OD', 'b93bc3c5-7611-4ff3-a699-0c2f7ecdd074', '+385991234567', false, false, NULL, true, 0, 0, '2025-10-01', '2025-10-01', false);

INSERT INTO "ContactInfos" ("Id", "FullName", "DateOfBirth", "Phone", "LanguageCode", "Email", "Gender", "GooglePlaceId", "FullAddress", "Latitude", "Longitude", "CityId", "CityName", "State", "PostalCode", "Country", "CreatedAt")
VALUES
(1, 'Helpi Admin', '1990-01-01', '+385991234567', 'hr', 'info@helpi.social', 0, 'ChIJplaceid00', 'Ilica 1, Zagreb', 45.8131, 15.9775, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2025-10-01');

INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT 1, "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'ADMIN';

-- ============================================
-- 1. USERS - STUDENTS (7 studenata)
-- ============================================
-- UserType: 0=Admin, 1=Student, 2=Customer

INSERT INTO "AspNetUsers" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount", "UserType", "CreatedAt", "UpdatedAt", "IsSuspended")
VALUES 
(101, 'luka.peric@email.com', 'LUKA.PERIC@EMAIL.COM', 'luka.peric@email.com', 'LUKA.PERIC@EMAIL.COM', true, 'AQAAAAIAAYagAAAAEFakeHashForTestingOnly1234567890abcdef', 'SECSTAMP101', 'CONCUR101', '+385991112222', false, false, NULL, true, 0, 1, '2025-11-01', '2025-11-01', false),
(102, 'ana.matic@email.com', 'ANA.MATIC@EMAIL.COM', 'ana.matic@email.com', 'ANA.MATIC@EMAIL.COM', true, 'AQAAAAIAAYagAAAAEFakeHashForTestingOnly1234567890abcdef', 'SECSTAMP102', 'CONCUR102', '+385993334444', false, false, NULL, true, 0, 1, '2025-12-15', '2025-12-15', false),
(103, 'ivan.simic@email.com', 'IVAN.SIMIC@EMAIL.COM', 'ivan.simic@email.com', 'IVAN.SIMIC@EMAIL.COM', true, 'AQAAAAIAAYagAAAAEFakeHashForTestingOnly1234567890abcdef', 'SECSTAMP103', 'CONCUR103', '+385995556666', false, false, NULL, true, 0, 1, '2026-01-10', '2026-01-10', false),
(104, 'petra.novak@email.com', 'PETRA.NOVAK@EMAIL.COM', 'petra.novak@email.com', 'PETRA.NOVAK@EMAIL.COM', true, 'AQAAAAIAAYagAAAAEFakeHashForTestingOnly1234567890abcdef', 'SECSTAMP104', 'CONCUR104', '+385997778888', false, false, NULL, true, 0, 1, '2026-02-20', '2026-02-20', false),
(105, 'marko.vukovic@email.com', 'MARKO.VUKOVIC@EMAIL.COM', 'marko.vukovic@email.com', 'MARKO.VUKOVIC@EMAIL.COM', true, 'AQAAAAIAAYagAAAAEFakeHashForTestingOnly1234567890abcdef', 'SECSTAMP105', 'CONCUR105', '+385992223333', false, false, NULL, true, 0, 1, '2025-10-20', '2025-10-20', false),
(106, 'maja.knezevic@email.com', 'MAJA.KNEZEVIC@EMAIL.COM', 'maja.knezevic@email.com', 'MAJA.KNEZEVIC@EMAIL.COM', true, 'AQAAAAIAAYagAAAAEFakeHashForTestingOnly1234567890abcdef', 'SECSTAMP106', 'CONCUR106', '+385994445555', false, false, NULL, true, 0, 1, '2025-11-15', '2025-11-15', false),
(107, 'dino.barisic@email.com', 'DINO.BARISIC@EMAIL.COM', 'dino.barisic@email.com', 'DINO.BARISIC@EMAIL.COM', true, 'AQAAAAIAAYagAAAAEFakeHashForTestingOnly1234567890abcdef', 'SECSTAMP107', 'CONCUR107', '+385996667777', false, false, NULL, true, 0, 1, '2026-01-05', '2026-01-05', false);

-- ============================================
-- 2. CONTACT INFO - STUDENTS
-- ============================================
-- Gender: 0=Male, 1=Female

-- Cities (required FK for ContactInfos — skip if already seeded by backend)
INSERT INTO "Cities" ("Id", "GooglePlaceId", "Name", "PostalCode", "IsServiced", "CreatedAt")
VALUES
(1, 'ChIJSx5MG-zaSRMRGkfexFjHAAQ', 'Split', '21000', true, '2025-01-01'),
(2, 'ChIJ0YaYlvBKZUcRIDqSxsEpBDg', 'Zagreb', '10000', true, '2025-01-01'),
(3, 'ChIJkUKS6XFbZUcR8MFjTIE3M3g', 'Rijeka', '51000', true, '2025-01-01'),
(4, 'ChIJPc9dJ37MQBMRKqTBG87xLwQ', 'Osijek', '31000', true, '2025-01-01')
ON CONFLICT ("Id") DO NOTHING;

SELECT setval('"Cities_Id_seq"', (SELECT MAX("Id") FROM "Cities"));

INSERT INTO "ContactInfos" ("Id", "FullName", "DateOfBirth", "Phone", "LanguageCode", "Email", "Gender", "GooglePlaceId", "FullAddress", "Latitude", "Longitude", "CityId", "CityName", "State", "PostalCode", "Country", "CreatedAt")
VALUES
(101, 'Luka Peric', '2002-05-14', '+385991112222', 'hr', 'luka.peric@email.com', 0, 'ChIJplaceid1', 'Trg bana Jelacica 1, Zagreb', 45.8131, 15.9775, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2025-11-01'),
(102, 'Ana Matic', '2003-08-22', '+385993334444', 'hr', 'ana.matic@email.com', 1, 'ChIJplaceid2', 'Ozaljska 55, Zagreb', 45.8000, 15.9500, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2025-12-15'),
(103, 'Ivan Simic', '2001-11-03', '+385995556666', 'hr', 'ivan.simic@email.com', 0, 'ChIJplaceid3', 'Dubrava 120, Zagreb', 45.8300, 16.0500, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2026-01-10'),
(104, 'Petra Novak', '2004-02-10', '+385997778888', 'hr', 'petra.novak@email.com', 1, 'ChIJplaceid4', 'Crnomerec 30, Zagreb', 45.8150, 15.9400, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2026-02-20'),
(105, 'Marko Vukovic', '2002-09-05', '+385992223333', 'hr', 'marko.vukovic@email.com', 0, 'ChIJplaceid5', 'Draskoviceva 18, Zagreb', 45.8120, 15.9800, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2025-10-20'),
(106, 'Maja Knezevic', '2003-04-17', '+385994445555', 'hr', 'maja.knezevic@email.com', 1, 'ChIJplaceid6', 'Tratinska 42, Zagreb', 45.8050, 15.9600, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2025-11-15'),
(107, 'Dino Barisic', '2001-12-30', '+385996667777', 'hr', 'dino.barisic@email.com', 0, 'ChIJplaceid7', 'Klaiceva 5, Zagreb', 45.8100, 15.9700, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2026-01-05');

-- ============================================
-- 3. STUDENTS (mixed statuses)
-- ============================================
-- StudentStatus: 0=InActive, 1=Active, 2=ContractAboutToExpire, 3=Expired, 4=AccountDeactivated

INSERT INTO "Students" ("UserId", "StudentNumber", "FacultyId", "DateRegistered", "ContactId", "Status", "TotalReviews", "TotalRatingSum", "AverageRating")
VALUES
(101, '0036512345', 1, '2025-11-01', 101, 1, 12, 57.6, 4.8),
(102, '0036598765', 2, '2025-12-15', 102, 1, 8, 36.8, 4.6),
(103, '0036554321', 3, '2026-01-10', 103, 1, 5, 21.0, 4.2),
(104, '0036567890', 4, '2026-02-20', 104, 2, 3, 15.0, 5.0),
(105, '0036511111', 5, '2025-10-20', 105, 0, 0, 0.0, 0.0),
(106, '0036522222', 6, '2025-11-15', 106, 4, 10, 49.0, 4.9),
(107, '0036533333', 7, '2026-01-05', 107, 3, 6, 25.8, 4.3);

-- ============================================
-- 4. USERS - CUSTOMERS (6 customera za 6 seniora)
-- ============================================

INSERT INTO "AspNetUsers" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount", "UserType", "CreatedAt", "UpdatedAt", "IsSuspended")
VALUES 
(201, 'ivka.mandic@email.com', 'IVKA.MANDIC@EMAIL.COM', 'ivka.mandic@email.com', 'IVKA.MANDIC@EMAIL.COM', true, 'AQAAAAIAAYagAAAAEFakeHashForTestingOnly1234567890abcdef', 'SECSTAMP201', 'CONCUR201', '+385912345678', false, false, NULL, true, 0, 2, '2026-01-15', '2026-01-15', false),
(202, 'ana.horvat@email.com', 'ANA.HORVAT@EMAIL.COM', 'ana.horvat@email.com', 'ANA.HORVAT@EMAIL.COM', true, 'AQAAAAIAAYagAAAAEFakeHashForTestingOnly1234567890abcdef', 'SECSTAMP202', 'CONCUR202', '+385987654321', false, false, NULL, true, 0, 2, '2026-01-20', '2026-01-20', false),
(203, 'josip.kovacevic@email.com', 'JOSIP.KOVACEVIC@EMAIL.COM', 'josip.kovacevic@email.com', 'JOSIP.KOVACEVIC@EMAIL.COM', true, 'AQAAAAIAAYagAAAAEFakeHashForTestingOnly1234567890abcdef', 'SECSTAMP203', 'CONCUR203', '+385914567890', false, false, NULL, true, 0, 2, '2026-02-01', '2026-02-01', false),
(204, 'kata.babic@email.com', 'KATA.BABIC@EMAIL.COM', 'kata.babic@email.com', 'KATA.BABIC@EMAIL.COM', true, 'AQAAAAIAAYagAAAAEFakeHashForTestingOnly1234567890abcdef', 'SECSTAMP204', 'CONCUR204', '+385925678901', false, false, NULL, true, 0, 2, '2026-02-10', '2026-02-10', false),
(205, 'franjo.juric@email.com', 'FRANJO.JURIC@EMAIL.COM', 'franjo.juric@email.com', 'FRANJO.JURIC@EMAIL.COM', true, 'AQAAAAIAAYagAAAAEFakeHashForTestingOnly1234567890abcdef', 'SECSTAMP205', 'CONCUR205', '+385916789012', false, false, NULL, true, 0, 2, '2025-12-05', '2025-12-05', false),
(206, 'ankica.tomic@email.com', 'ANKICA.TOMIC@EMAIL.COM', 'ankica.tomic@email.com', 'ANKICA.TOMIC@EMAIL.COM', true, 'AQAAAAIAAYagAAAAEFakeHashForTestingOnly1234567890abcdef', 'SECSTAMP206', 'CONCUR206', '+385917890123', false, false, NULL, true, 0, 2, '2026-02-20', '2026-02-20', false);

-- ============================================
-- 5. CONTACT INFO - CUSTOMERS
-- ============================================

INSERT INTO "ContactInfos" ("Id", "FullName", "DateOfBirth", "Phone", "LanguageCode", "Email", "Gender", "GooglePlaceId", "FullAddress", "Latitude", "Longitude", "CityId", "CityName", "State", "PostalCode", "Country", "CreatedAt")
VALUES
(201, 'Ivka Mandic', '1948-07-22', '+385912345678', 'hr', 'ivka.mandic@email.com', 1, 'ChIJplaceid11', 'Ilica 45, Zagreb', 45.8140, 15.9600, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2026-01-15'),
(202, 'Ana Horvat', '1985-03-15', '+385987654321', 'hr', 'ana.horvat@email.com', 1, 'ChIJplaceid12', 'Savska 77, Zagreb', 45.8000, 15.9700, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2026-01-20'),
(203, 'Josip Kovacevic', '1940-05-18', '+385914567890', 'hr', 'josip.kovacevic@email.com', 0, 'ChIJplaceid13', 'Maksimirska 100, Zagreb', 45.8200, 16.0200, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2026-02-01'),
(204, 'Kata Babic', '1945-09-12', '+385925678901', 'hr', 'kata.babic@email.com', 1, 'ChIJplaceid14', 'Savska 25, Zagreb', 45.8050, 15.9650, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2026-02-10'),
(205, 'Franjo Juric', '1938-02-28', '+385916789012', 'hr', 'franjo.juric@email.com', 0, 'ChIJplaceid15', 'Heinzelova 8, Zagreb', 45.8180, 16.0000, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2025-12-05'),
(206, 'Ankica Tomic', '1950-04-10', '+385917890123', 'hr', 'ankica.tomic@email.com', 1, 'ChIJplaceid16', 'Draskoviceva 33, Zagreb', 45.8125, 15.9820, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2026-02-20');

-- ============================================
-- 6. CUSTOMERS
-- ============================================

INSERT INTO "Customers" ("UserId", "ContactId", "CreatedAt", "PreferredNotificationMethod")
VALUES
(201, 201, '2026-01-15', 0),
(202, 202, '2026-01-20', 0),
(203, 203, '2026-02-01', 0),
(204, 204, '2026-02-10', 0),
(205, 205, '2025-12-05', 0),
(206, 206, '2026-02-20', 0);

-- ============================================
-- 7. CONTACT INFO - SENIORS
-- ============================================

INSERT INTO "ContactInfos" ("Id", "FullName", "DateOfBirth", "Phone", "LanguageCode", "Email", "Gender", "GooglePlaceId", "FullAddress", "Latitude", "Longitude", "CityId", "CityName", "State", "PostalCode", "Country", "CreatedAt")
VALUES
(301, 'Ivka Mandic', '1948-07-22', '+385912345678', 'hr', 'ivka.mandic@email.com', 1, 'ChIJplaceid21', 'Ilica 45, Zagreb', 45.8140, 15.9600, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2026-01-15'),
(302, 'Marija Horvat', '1942-11-03', '+385923456789', 'hr', 'marija.horvat@email.com', 1, 'ChIJplaceid22', 'Vukovarska 12, Zagreb', 45.8070, 15.9900, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2026-01-20'),
(303, 'Josip Kovacevic', '1940-05-18', '+385914567890', 'hr', 'josip.kovacevic@email.com', 0, 'ChIJplaceid23', 'Maksimirska 100, Zagreb', 45.8200, 16.0200, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2026-02-01'),
(304, 'Kata Babic', '1945-09-12', '+385925678901', 'hr', 'kata.babic@email.com', 1, 'ChIJplaceid24', 'Savska 25, Zagreb', 45.8050, 15.9650, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2026-02-10'),
(305, 'Franjo Juric', '1938-02-28', '+385916789012', 'hr', 'franjo.juric@email.com', 0, 'ChIJplaceid25', 'Heinzelova 8, Zagreb', 45.8180, 16.0000, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2025-12-05'),
(306, 'Ankica Tomic', '1950-04-10', '+385917890123', 'hr', 'ankica.tomic@email.com', 1, 'ChIJplaceid26', 'Draskoviceva 33, Zagreb', 45.8125, 15.9820, 2, 'Zagreb', 'Zagreb', '10000', 'Croatia', '2026-02-20');

-- ============================================
-- 8. SENIORS
-- ============================================
-- Relationship: 0=Self, 1=Spouse, 2=Parent, 3=Relative, 4=Other
-- Senior 2 (Marija) ima orderera (Customer 202) - relationship=2 (Parent)
-- Senior 4 (Kata) ima orderera (Customer 204) - relationship=3 (Relative)
-- Senior 6 (Ankica) ima orderera (Customer 206) - relationship=2 (Parent)

INSERT INTO "Seniors" ("Id", "CustomerId", "ContactId", "Relationship", "CreatedAt", "TotalReviews", "TotalRatingSum", "AverageRating")
VALUES
(1, 201, 301, 0, '2026-01-15', 0, 0, 0),
(2, 202, 302, 2, '2026-01-20', 0, 0, 0),
(3, 203, 303, 0, '2026-02-01', 0, 0, 0),
(4, 204, 304, 3, '2026-02-10', 0, 0, 0),
(5, 205, 305, 0, '2025-12-05', 0, 0, 0),
(6, 206, 306, 2, '2026-02-20', 0, 0, 0);

-- ============================================
-- 9. ORDERS (mixed statuses)
-- ============================================
-- OrderStatus: 0=InActive, 1=Pending, 2=FullAssigned, 3=Completed, 4=Cancelled

-- Jednokratne narudzbe
INSERT INTO "Orders" ("Id", "SeniorId", "Status", "IsRecurring", "RecurrencePattern", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
VALUES
(1, 1, 3, false, NULL, '2026-03-20', '2026-03-20', 'Mlijeko i kruh iz Konzuma, lijekove iz ljekarne.', '2026-03-01 10:30:00', '2026-03-20 14:00:00'),
(2, 3, 1, false, NULL, '2026-03-22', '2026-03-22', 'Pratnja na kontrolu kod lijecnika.', '2026-03-03 11:00:00', '2026-03-03 11:00:00'),
(3, 1, 3, false, NULL, '2026-03-25', '2026-03-25', 'Kupovina namirnica za cijeli tjedan.', '2026-03-05 09:00:00', '2026-03-25 18:00:00'),
(4, 3, 3, false, NULL, '2026-04-01', '2026-04-01', 'Setnja u parku i druzenje.', '2026-03-05 10:00:00', '2026-04-01 13:00:00'),
(5, 6, 2, false, NULL, '2026-04-10', '2026-04-10', 'Druzenje i razgovor uz kavu.', '2026-03-05 11:00:00', '2026-03-05 11:00:00');

-- Ponavljajuce narudzbe
INSERT INTO "Orders" ("Id", "SeniorId", "Status", "IsRecurring", "RecurrencePattern", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
VALUES
(6, 2, 2, true, 0, '2026-03-17', '2026-06-30', 'Pomoc s ciscenjem i druzenje.', '2026-03-01 14:00:00', '2026-03-01 14:00:00'),
(7, 3, 2, true, 0, '2026-03-18', '2026-06-30', 'Setnja i druzenje.', '2026-02-20 09:00:00', '2026-02-20 09:00:00'),
(8, 4, 2, true, 0, '2026-03-17', '2026-04-30', 'Pratnja do lijecnika svaki utorak.', '2026-02-15 11:30:00', '2026-02-15 11:30:00'),
(9, 1, 2, true, 0, '2026-03-17', '2026-05-31', 'Kupovina i druzenje ponedjeljkom i cetvrtkom.', '2026-02-25 10:00:00', '2026-02-25 10:00:00'),
(10, 2, 3, true, 0, '2026-01-06', '2026-02-28', 'Setnja u Maksimiru utorkom i petkom.', '2025-12-20 14:00:00', '2026-02-28 18:00:00'),
(11, 4, 4, true, 0, '2026-03-17', '2026-06-30', 'Pomoc s ciscenjem utorkom i cetvrtkom.', '2026-03-04 08:30:00', '2026-03-10 09:00:00');

-- differentTimes test orders (recurring, multiple schedules, partial student overlaps)
INSERT INTO "Orders" ("Id", "SeniorId", "Status", "IsRecurring", "RecurrencePattern", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
VALUES
(12, 5, 1, true, 0, '2026-03-23', '2026-06-30', 'Pomoc u kucanstvu ponedjeljkom i cetvrtkom ujutro.', '2026-03-10 10:00:00', '2026-03-10 10:00:00'),
(13, 4, 1, true, 0, '2026-03-23', '2026-06-30', 'Druzenje i razgovor ponedjeljkom i cetvrtkom oko podneva.', '2026-03-10 11:00:00', '2026-03-10 11:00:00'),
(14, 6, 1, true, 0, '2026-03-17', '2026-05-31', 'Kupovina utorkom i subotom ujutro.', '2026-03-10 12:00:00', '2026-03-10 12:00:00'),
(15, 3, 1, true, 0, '2026-04-06', '2026-06-30', 'Druzenje i razgovor ponedjeljkom i utorkom.', NOW(), NOW());

-- ============================================
-- 10. ORDER SCHEDULES
-- ============================================
-- DayOfWeek: 0=Sunday, 1=Monday, 2=Tuesday, 3=Wednesday, 4=Thursday, 5=Friday, 6=Saturday

-- Jednokratne (1 schedule each)
-- Order 1: Thu 10-12, Sr 11 (shopping) → Ana(Thu 09-15 ✓, svc 11✓), Petra(Thu 08-18 ✓, svc 11✓)
-- Order 2: Sun 09-12, Sr 31 (escort) → NOBODY has Sunday! Unassignable test case
-- Order 3: Tue 14-17, Sr 11 (shopping) → Petra only (Ana Tue ends 15:00 < 17:00)
-- Order 4: Wed 10-12, Sr 4 (walking) → Luka(Wed ✓) + Petra(Wed ✓). Ivan excluded by Order 7 conflict!
-- Order 5: Fri 08-11, Sr 1 (companionship) → Luka(Fri 08-14 ✓), Petra(Fri 08-18 ✓). NOT Ivan(Fri 10-17, 08<10)

INSERT INTO "OrderSchedules" ("Id", "OrderId", "DayOfWeek", "StartTime", "EndTime", "IsCancelled", "AutoScheduleAttemptCount", "AllowAutoScheduling")
VALUES
(1, 1, 4, '10:00:00', '12:00:00', false, 0, true),
(2, 2, 0, '09:00:00', '12:00:00', false, 0, true),
(3, 3, 2, '14:00:00', '17:00:00', false, 0, true),
(4, 4, 3, '10:00:00', '12:00:00', false, 0, true),
(5, 5, 5, '08:00:00', '11:00:00', false, 0, true);

-- Ponavljajuce
-- Order 6 (FullAssigned): Thu+Sat, Sr 21 (house help) → Ana assigned (Thu 09-15 ✓, Sat 10-14 ✓, svc 21✓)
-- Order 7 (FullAssigned): Wed+Fri 10-12, Sr 4 (walking) → Ivan assigned (Wed+Fri 10-17 ✓, svc 4✓)
-- Order 8 (Pending): Tue 08:30-12:30, Sr 31 (escort) → Only Petra (Tue 08-18 ✓, svc 31✓; Ana Tue starts 09:00 > 08:30)
-- Order 9 (Pending): Mon+Thu 09-11, Sr 11+1 → Only Petra (others lack svc 11 or Mon availability)
-- Order 10 (Completed): Tue+Fri 10-12, Sr 4 (walking) → Petra was assigned (completed)
-- Order 11 (Cancelled): Tue+Thu 11-13, Sr 41 (other) → Cancelled, no assignments

INSERT INTO "OrderSchedules" ("Id", "OrderId", "DayOfWeek", "StartTime", "EndTime", "IsCancelled", "AutoScheduleAttemptCount", "AllowAutoScheduling")
VALUES
(6, 6, 4, '09:00:00', '12:00:00', false, 0, true),
(7, 6, 6, '10:00:00', '13:00:00', false, 0, true),
(8, 7, 3, '10:00:00', '12:00:00', false, 0, true),
(9, 7, 5, '10:00:00', '12:00:00', false, 0, true),
(10, 8, 2, '08:30:00', '12:30:00', false, 0, true),
(11, 9, 1, '09:00:00', '11:00:00', false, 0, true),
(12, 9, 4, '09:00:00', '11:00:00', false, 0, true),
(13, 10, 2, '10:00:00', '12:00:00', false, 0, true),
(14, 10, 5, '10:00:00', '12:00:00', false, 0, true),
(15, 11, 2, '11:00:00', '13:00:00', true, 0, false),
(16, 11, 4, '11:00:00', '13:00:00', true, 0, false),
-- Order 12 (differentTimes test #1): Mon+Thu 09-12, walking
-- Backend returns Luka+Petra (Mon ok). Frontend: Luka=differentTimes(Thu 13-17≠09:00), Petra=full
(17, 12, 1, '09:00:00', '12:00:00', false, 0, true),
(18, 12, 4, '09:00:00', '12:00:00', false, 0, true),
-- Order 13 (differentTimes test #2): Mon+Thu 11-13, walking
-- Backend returns Luka+Ivan+Petra (Mon ok). Frontend: Luka=diff(Thu), Ivan=diff(Thu 12-17≠11:00), Petra=full
(19, 13, 1, '11:00:00', '13:00:00', false, 0, true),
(20, 13, 4, '11:00:00', '13:00:00', false, 0, true),
-- Order 14 (differentTimes test #3): Tue+Sat 09-12, companionship
-- Backend returns Ana+Petra (Tue ok). Frontend: Ana=diff(Sat 10-14≠09:00), Petra=unavailable(no Sat)
(21, 14, 2, '09:00:00', '12:00:00', false, 0, true),
(22, 14, 6, '09:00:00', '12:00:00', false, 0, true),
-- Order 15 (differentTimes test #4): Mon+Tue 12-14, Thu 10-12, companionship
-- Luka: Mon 12-14=GREEN (has Mon 08-14), Tue 12-14=RED no alt (no Tue slot), Thu 10-12=RED WITH alt (has Thu 13-17→offers 13:00,14:00,15:00)
-- Ana: Mon+Tue+Thu → full, Petra: Mon+Tue+Thu → full
(23, 15, 1, '12:00:00', '14:00:00', false, 0, false),
(24, 15, 2, '12:00:00', '14:00:00', false, 0, false),
(25, 15, 4, '10:00:00', '12:00:00', false, 0, false);

-- ============================================
-- 11. ORDER SERVICES
-- ============================================
-- 6 servisa: 1=Društvo, 4=Šetnja, 11=Kupovina, 21=Kućanstvo, 31=Pratnja, 41=Ostalo

INSERT INTO "OrderServices" ("OrderId", "ServiceId") VALUES
(1, 11),
(2, 31),
(3, 11),
(4, 4),
(5, 1),
(6, 21),
(7, 4),
(8, 31),
(9, 11), (9, 1),
(10, 4),
(11, 41),
(12, 21),
(13, 1),
(14, 11),
(15, 1);

-- ============================================
-- 12. STUDENT SERVICES
-- ============================================
-- Luka (101): Društvo + Šetnja
-- Ana (102): Društvo + Šetnja + Kupovina + Kućanstvo
-- Ivan (103): Šetnja + Pratnja + Ostalo
-- Petra (104): Društvo + Šetnja + Kupovina + Pratnja (most versatile)
-- Dino (107): Kućanstvo + Ostalo (expired, can't be assigned)

INSERT INTO "StudentServices" ("StudentId", "ServiceId", "ExperienceYears") VALUES
(101, 1, 2), (101, 4, 1),
(102, 1, 1), (102, 4, 1), (102, 11, 2), (102, 21, 1),
(103, 4, 2), (103, 31, 1), (103, 41, 1),
(104, 1, 1), (104, 4, 2), (104, 11, 1), (104, 31, 1),
(107, 21, 1), (107, 41, 1);

-- ============================================
-- 12b. STUDENT CONTRACTS
-- ============================================
-- ContractNumber format: HLP-YYYY-MM
-- Active students (101-103) have contracts valid for next 6 months
-- Student 104 (AboutToExpire) has contract expiring in 3 days!
-- Student 107 (Expired) has expired contract from last year

INSERT INTO "StudentContracts" ("Id", "StudentId", "ContractNumber", "CloudPath", "EffectiveDate", "ExpirationDate", "UploadedAt")
VALUES
(1, 101, 'HLP-2025-11', '/contracts/hlp-2025-11-101-2025.pdf', '2025-11-01', '2026-11-01', '2025-11-01 10:00:00'),
(2, 102, 'HLP-2025-12', '/contracts/hlp-2025-12-102-2025.pdf', '2025-12-15', '2026-12-15', '2025-12-15 10:00:00'),
(3, 103, 'HLP-2026-01', '/contracts/hlp-2026-01-103-2026.pdf', '2026-01-10', '2027-01-10', '2026-01-10 10:00:00'),
(4, 104, 'HLP-2026-02', '/contracts/hlp-2026-02-104-2026.pdf', '2026-02-20', '2026-03-22', '2026-02-20 10:00:00'),
(5, 107, 'HLP-2025-06', '/contracts/hlp-2025-06-107-2025.pdf', '2025-06-01', '2025-12-01', '2025-06-01 10:00:00');

-- ============================================
-- 13. STUDENT AVAILABILITY SLOTS
-- ============================================
-- DayOfWeek: 0=Sunday, 1=Monday, 2=Tuesday, 3=Wednesday, 4=Thursday, 5=Friday, 6=Saturday
-- Luka (101): Mon/Wed/Fri 08-14 + Thu 13-17  (morning except Thu afternoon!)
-- Ana (102): Mon 12-15, Tue/Thu 09-15, Sat 10-14  (Mon only afternoon!)
-- Ivan (103): Mon/Wed/Fri 10-17 + Tue/Thu 12-17  (Tue/Thu only afternoon!)
-- Petra (104): Mon-Fri 08-18  (full availability, most flexible, NO weekend)
-- Dino (107): Tue/Thu 09-13  (expired but slots remain)

INSERT INTO "StudentAvailabilitySlots" ("StudentId", "DayOfWeek", "StartTime", "EndTime") VALUES
(101, 1, '08:00:00', '14:00:00'),
(101, 3, '08:00:00', '14:00:00'),
(101, 4, '13:00:00', '17:00:00'),
(101, 5, '08:00:00', '14:00:00'),
(102, 1, '12:00:00', '15:00:00'),
(102, 2, '09:00:00', '15:00:00'),
(102, 4, '09:00:00', '15:00:00'),
(102, 6, '10:00:00', '14:00:00'),
(103, 1, '10:00:00', '17:00:00'),
(103, 2, '12:00:00', '17:00:00'),
(103, 3, '10:00:00', '17:00:00'),
(103, 4, '12:00:00', '17:00:00'),
(103, 5, '10:00:00', '17:00:00'),
(104, 1, '08:00:00', '18:00:00'),
(104, 2, '08:00:00', '18:00:00'),
(104, 3, '08:00:00', '18:00:00'),
(104, 4, '08:00:00', '18:00:00'),
(104, 5, '08:00:00', '18:00:00'),
(107, 2, '09:00:00', '13:00:00'),
(107, 4, '09:00:00', '13:00:00');

-- ============================================
-- 14. SCHEDULE ASSIGNMENTS (for FullAssigned + Completed orders)
-- ============================================
-- AssignmentStatus: 0=Accepted, 1=Declined, 2=Terminated, 3=Completed

-- Order 6 (FullAssigned) → Ana (102) assigned to both schedules
INSERT INTO "ScheduleAssignments" ("Id", "OrderScheduleId", "OrderId", "StudentId", "Status", "IsJobInstanceSub", "AssignedAt", "AcceptedAt")
VALUES
(1, 6, 6, 102, 0, false, '2026-03-10 09:00:00', '2026-03-10 09:00:00'),
(2, 7, 6, 102, 0, false, '2026-03-10 09:05:00', '2026-03-10 09:05:00');

-- Order 7 (FullAssigned) → Ivan (103) assigned to both schedules
INSERT INTO "ScheduleAssignments" ("Id", "OrderScheduleId", "OrderId", "StudentId", "Status", "IsJobInstanceSub", "AssignedAt", "AcceptedAt")
VALUES
(3, 8, 7, 103, 0, false, '2026-03-12 10:00:00', '2026-03-12 10:00:00'),
(4, 9, 7, 103, 0, false, '2026-03-12 10:05:00', '2026-03-12 10:05:00');

-- Order 10 (Completed) → Petra (104) assigned + completed both schedules
INSERT INTO "ScheduleAssignments" ("Id", "OrderScheduleId", "OrderId", "StudentId", "Status", "IsJobInstanceSub", "AssignedAt", "AcceptedAt", "CompletedAt")
VALUES
(5, 13, 10, 104, 3, false, '2026-01-02 08:00:00', '2026-01-02 08:00:00', '2026-02-28 18:00:00'),
(6, 14, 10, 104, 3, false, '2026-01-02 08:05:00', '2026-01-02 08:05:00', '2026-02-28 18:00:00');

-- Order 1 (Completed, one-time) → Ana (102) assigned + completed, Thu 10-12
INSERT INTO "ScheduleAssignments" ("Id", "OrderScheduleId", "OrderId", "StudentId", "Status", "IsJobInstanceSub", "AssignedAt", "AcceptedAt", "CompletedAt")
VALUES
(7, 1, 1, 102, 3, false, '2026-03-10 10:00:00', '2026-03-10 10:00:00', '2026-03-20 12:00:00');

-- Order 3 (Completed, one-time) → Petra (104) assigned + completed, Tue 14-17
INSERT INTO "ScheduleAssignments" ("Id", "OrderScheduleId", "OrderId", "StudentId", "Status", "IsJobInstanceSub", "AssignedAt", "AcceptedAt", "CompletedAt")
VALUES
(8, 3, 3, 104, 3, false, '2026-03-12 09:00:00', '2026-03-12 09:00:00', '2026-03-25 17:00:00');

-- Order 4 (Completed, one-time) → Luka (101) assigned + completed, Wed 10-12
INSERT INTO "ScheduleAssignments" ("Id", "OrderScheduleId", "OrderId", "StudentId", "Status", "IsJobInstanceSub", "AssignedAt", "AcceptedAt", "CompletedAt")
VALUES
(9, 4, 4, 101, 3, false, '2026-03-12 10:00:00', '2026-03-12 10:00:00', '2026-04-01 12:00:00');

-- Order 5 (FullAssigned, one-time) → Luka (101) assigned, Fri 08-11
INSERT INTO "ScheduleAssignments" ("Id", "OrderScheduleId", "OrderId", "StudentId", "Status", "IsJobInstanceSub", "AssignedAt", "AcceptedAt")
VALUES
(10, 5, 5, 101, 0, false, '2026-03-15 08:00:00', '2026-03-15 08:00:00');

-- Order 8 (FullAssigned, recurring) → Petra (104) assigned, Tue 08:30-12:30
INSERT INTO "ScheduleAssignments" ("Id", "OrderScheduleId", "OrderId", "StudentId", "Status", "IsJobInstanceSub", "AssignedAt", "AcceptedAt")
VALUES
(11, 10, 8, 104, 0, false, '2026-03-10 11:00:00', '2026-03-10 11:00:00');

-- Order 9 (FullAssigned, recurring) → Petra (104) assigned, Mon 09-11 + Thu 09-11
INSERT INTO "ScheduleAssignments" ("Id", "OrderScheduleId", "OrderId", "StudentId", "Status", "IsJobInstanceSub", "AssignedAt", "AcceptedAt")
VALUES
(12, 11, 9, 104, 0, false, '2026-03-10 12:00:00', '2026-03-10 12:00:00'),
(13, 12, 9, 104, 0, false, '2026-03-10 12:05:00', '2026-03-10 12:05:00');

-- ============================================
-- 15. JOB INSTANCES (sessions)
-- ============================================
-- JobInstanceStatus: 0=Upcoming, 1=InProgress, 2=Completed, 3=Cancelled, 4=Rescheduled
-- PaymentStatus: 0=Pending

-- Order 6 (Ana=102, Senior=2, Customer=202): Thu 09-12 + Sat 10-13, Ozujak 2026
INSERT INTO "JobInstances" ("Id","SeniorId","CustomerId","OrderId","OrderScheduleId","ScheduleAssignmentId","ScheduledDate","StartTime","EndTime","Status","NeedsSubstitute","IsRescheduleVariant","HourlyRate","CompanyPercentage","ServiceProviderPercentage","PaymentStatus")
VALUES
(1, 2, 202, 6, 6, 1, '2026-03-19', '09:00:00','12:00:00', 0, false, false, 8.00, 30, 70, 0),
(2, 2, 202, 6, 7, 2, '2026-03-21', '10:00:00','13:00:00', 0, false, false, 8.00, 30, 70, 0),
(3, 2, 202, 6, 6, 1, '2026-03-26', '09:00:00','12:00:00', 0, false, false, 8.00, 30, 70, 0),
(4, 2, 202, 6, 7, 2, '2026-03-28', '10:00:00','13:00:00', 0, false, false, 8.00, 30, 70, 0),
-- April
(5, 2, 202, 6, 6, 1, '2026-04-02', '09:00:00','12:00:00', 0, false, false, 8.00, 30, 70, 0),
(6, 2, 202, 6, 7, 2, '2026-04-04', '10:00:00','13:00:00', 0, false, false, 8.00, 30, 70, 0);

-- Order 7 (Ivan=103, Senior=3, Customer=203): Wed 10-12 + Fri 10-12, Ozujak 2026
INSERT INTO "JobInstances" ("Id","SeniorId","CustomerId","OrderId","OrderScheduleId","ScheduleAssignmentId","ScheduledDate","StartTime","EndTime","Status","NeedsSubstitute","IsRescheduleVariant","HourlyRate","CompanyPercentage","ServiceProviderPercentage","PaymentStatus")
VALUES
(7,  3, 203, 7, 8, 3, '2026-03-18', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(8,  3, 203, 7, 9, 4, '2026-03-20', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(9,  3, 203, 7, 8, 3, '2026-03-25', '10:00:00','12:00:00', 0, false, false, 8.00, 30, 70, 0),
(10, 3, 203, 7, 9, 4, '2026-03-27', '10:00:00','12:00:00', 0, false, false, 8.00, 30, 70, 0),
-- April
(11, 3, 203, 7, 8, 3, '2026-04-01', '10:00:00','12:00:00', 0, false, false, 8.00, 30, 70, 0),
(12, 3, 203, 7, 9, 4, '2026-04-03', '10:00:00','12:00:00', 0, false, false, 8.00, 30, 70, 0);

-- Order 10 (Petra=104, Senior=2, Customer=202): Tue 10-12 + Fri 10-12, Sijecanj-Veljaca (Completed)
INSERT INTO "JobInstances" ("Id","SeniorId","CustomerId","OrderId","OrderScheduleId","ScheduleAssignmentId","ScheduledDate","StartTime","EndTime","Status","NeedsSubstitute","IsRescheduleVariant","HourlyRate","CompanyPercentage","ServiceProviderPercentage","PaymentStatus")
VALUES
-- Sijecanj
(13, 2, 202, 10, 13, 5, '2026-01-06', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(14, 2, 202, 10, 14, 6, '2026-01-09', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(15, 2, 202, 10, 13, 5, '2026-01-13', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(16, 2, 202, 10, 14, 6, '2026-01-16', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(17, 2, 202, 10, 13, 5, '2026-01-20', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(18, 2, 202, 10, 14, 6, '2026-01-23', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(19, 2, 202, 10, 13, 5, '2026-01-27', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(20, 2, 202, 10, 14, 6, '2026-01-30', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
-- Veljaca
(21, 2, 202, 10, 13, 5, '2026-02-03', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(22, 2, 202, 10, 14, 6, '2026-02-06', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(23, 2, 202, 10, 13, 5, '2026-02-10', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(24, 2, 202, 10, 14, 6, '2026-02-13', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(25, 2, 202, 10, 13, 5, '2026-02-17', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(26, 2, 202, 10, 14, 6, '2026-02-20', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(27, 2, 202, 10, 13, 5, '2026-02-24', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0),
(28, 2, 202, 10, 14, 6, '2026-02-27', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0);

-- Order 1 (Ana=102, Senior=1, Customer=201): one-time Thu 2026-03-20 10-12 (Completed)
INSERT INTO "JobInstances" ("Id","SeniorId","CustomerId","OrderId","OrderScheduleId","ScheduleAssignmentId","ScheduledDate","StartTime","EndTime","Status","NeedsSubstitute","IsRescheduleVariant","HourlyRate","CompanyPercentage","ServiceProviderPercentage","PaymentStatus")
VALUES
(29, 1, 201, 1, 1, 7, '2026-03-20', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0);

-- Order 3 (Petra=104, Senior=1, Customer=201): one-time Tue 2026-03-25 14-17 (Completed)
INSERT INTO "JobInstances" ("Id","SeniorId","CustomerId","OrderId","OrderScheduleId","ScheduleAssignmentId","ScheduledDate","StartTime","EndTime","Status","NeedsSubstitute","IsRescheduleVariant","HourlyRate","CompanyPercentage","ServiceProviderPercentage","PaymentStatus")
VALUES
(30, 1, 201, 3, 3, 8, '2026-03-25', '14:00:00','17:00:00', 2, false, false, 8.00, 30, 70, 0);

-- Order 4 (Luka=101, Senior=3, Customer=203): one-time Wed 2026-04-01 10-12 (Completed)
INSERT INTO "JobInstances" ("Id","SeniorId","CustomerId","OrderId","OrderScheduleId","ScheduleAssignmentId","ScheduledDate","StartTime","EndTime","Status","NeedsSubstitute","IsRescheduleVariant","HourlyRate","CompanyPercentage","ServiceProviderPercentage","PaymentStatus")
VALUES
(31, 3, 203, 4, 4, 9, '2026-04-01', '10:00:00','12:00:00', 2, false, false, 8.00, 30, 70, 0);

-- Order 5 (Luka=101, Senior=6, Customer=206): one-time Fri 2026-04-10 08-11
INSERT INTO "JobInstances" ("Id","SeniorId","CustomerId","OrderId","OrderScheduleId","ScheduleAssignmentId","ScheduledDate","StartTime","EndTime","Status","NeedsSubstitute","IsRescheduleVariant","HourlyRate","CompanyPercentage","ServiceProviderPercentage","PaymentStatus")
VALUES
(32, 6, 206, 5, 5, 10, '2026-04-10', '08:00:00','11:00:00', 0, false, false, 8.00, 30, 70, 0);

-- Order 8 (Petra=104, Senior=4, Customer=204): Tue 08:30-12:30, Ozujak-Travanj 2026
INSERT INTO "JobInstances" ("Id","SeniorId","CustomerId","OrderId","OrderScheduleId","ScheduleAssignmentId","ScheduledDate","StartTime","EndTime","Status","NeedsSubstitute","IsRescheduleVariant","HourlyRate","CompanyPercentage","ServiceProviderPercentage","PaymentStatus")
VALUES
(33, 4, 204, 8, 10, 11, '2026-03-17', '08:30:00','12:30:00', 0, false, false, 8.00, 30, 70, 0),
(34, 4, 204, 8, 10, 11, '2026-03-24', '08:30:00','12:30:00', 0, false, false, 8.00, 30, 70, 0),
(35, 4, 204, 8, 10, 11, '2026-03-31', '08:30:00','12:30:00', 0, false, false, 8.00, 30, 70, 0),
(36, 4, 204, 8, 10, 11, '2026-04-07', '08:30:00','12:30:00', 0, false, false, 8.00, 30, 70, 0),
(37, 4, 204, 8, 10, 11, '2026-04-14', '08:30:00','12:30:00', 0, false, false, 8.00, 30, 70, 0),
(38, 4, 204, 8, 10, 11, '2026-04-21', '08:30:00','12:30:00', 0, false, false, 8.00, 30, 70, 0),
(39, 4, 204, 8, 10, 11, '2026-04-28', '08:30:00','12:30:00', 0, false, false, 8.00, 30, 70, 0);

-- Order 9 (Petra=104, Senior=1, Customer=201): Mon 09-11 + Thu 09-11, Ozujak-Travanj 2026
INSERT INTO "JobInstances" ("Id","SeniorId","CustomerId","OrderId","OrderScheduleId","ScheduleAssignmentId","ScheduledDate","StartTime","EndTime","Status","NeedsSubstitute","IsRescheduleVariant","HourlyRate","CompanyPercentage","ServiceProviderPercentage","PaymentStatus")
VALUES
-- Mon
(40, 1, 201, 9, 11, 12, '2026-03-17', '09:00:00','11:00:00', 0, false, false, 8.00, 30, 70, 0),
(41, 1, 201, 9, 11, 12, '2026-03-24', '09:00:00','11:00:00', 0, false, false, 8.00, 30, 70, 0),
(42, 1, 201, 9, 11, 12, '2026-03-31', '09:00:00','11:00:00', 0, false, false, 8.00, 30, 70, 0),
(43, 1, 201, 9, 11, 12, '2026-04-06', '09:00:00','11:00:00', 0, false, false, 8.00, 30, 70, 0),
-- Thu
(44, 1, 201, 9, 12, 13, '2026-03-19', '09:00:00','11:00:00', 0, false, false, 8.00, 30, 70, 0),
(45, 1, 201, 9, 12, 13, '2026-03-26', '09:00:00','11:00:00', 0, false, false, 8.00, 30, 70, 0),
(46, 1, 201, 9, 12, 13, '2026-04-02', '09:00:00','11:00:00', 0, false, false, 8.00, 30, 70, 0),
(47, 1, 201, 9, 12, 13, '2026-04-09', '09:00:00','11:00:00', 0, false, false, 8.00, 30, 70, 0);

-- ============================================
-- 16. RESET SEQUENCES
-- ============================================

SELECT setval('"AspNetUsers_Id_seq"', (SELECT MAX("Id") FROM "AspNetUsers"));
SELECT setval('"ContactInfos_Id_seq"', (SELECT MAX("Id") FROM "ContactInfos"));
SELECT setval('"Seniors_Id_seq"', (SELECT MAX("Id") FROM "Seniors"));
SELECT setval('"Orders_Id_seq"', (SELECT MAX("Id") FROM "Orders"));
SELECT setval('"OrderSchedules_Id_seq"', (SELECT MAX("Id") FROM "OrderSchedules"));
SELECT setval('"ScheduleAssignments_Id_seq"', (SELECT MAX("Id") FROM "ScheduleAssignments"));
SELECT setval('"StudentContracts_Id_seq"', (SELECT MAX("Id") FROM "StudentContracts"));
SELECT setval('"JobInstances_Id_seq"', (SELECT MAX("Id") FROM "JobInstances"));

COMMIT;

-- ============================================
-- SUMMARY
-- ============================================
-- 7 studenata (User 101-107)
--   Active: 101 Luka, 102 Ana, 103 Ivan
--   AboutToExpire: 104 Petra
--   InActive: 105 Marko (no contract)
--   Deactivated: 106 Maja
--   Expired: 107 Dino
--
-- 6 customera (User 201-206) → 6 seniora (Senior 1-6)
--
-- 14 narudzbi:
--   Pending(1): Orders 2,12,13,14,15 (5 orders)
--   FullAssigned(2): Orders 5,6,7,8,9 (5 orders)
--   Completed(3): Orders 1,3,4,10 (4 orders — 3 jednokratne + 1 ponavljajuca)
--   Cancelled(4): Order 11
--
-- 25 rasporeda (OrderSchedule 1-25)
-- 13 dodjela (ScheduleAssignment 1-13)
-- 47 job instanci (JobInstance 1-47)
--
-- REALISTICNA DISTRIBUCIJA PO SENIORU:
--   Ivka (Sr1):   1 aktivna (#9), 2 zavrsene (#1, #3)
--   Marija (Sr2): 1 aktivna (#6), 1 zavrsena (#10)
--   Josip (Sr3):  1 aktivna (#7), 1 zavrsena (#4), 1 u obradi (#2 ned.), 1 u obradi (#15)
--   Kata (Sr4):   1 aktivna (#8), 1 u obradi (#13), 1 otkazana (#11)
--   Franjo (Sr5): 1 u obradi (#12)
--   Ankica (Sr6): 1 aktivna (#5), 1 u obradi (#14)
-- 15 student-servisa
-- 20 availability slotova
--
-- FULL-MATCH SCENARIOS (all candidates = full):
--   Order 1 (Thu 10-12, shopping) → Ana + Petra match
--   Order 2 (Sun 09-12, escort)  → NOBODY matches! (Sunday test)
--   Order 3 (Tue 14-17, shopping) → Only Petra (Ana ends 15:00 < 17:00)
--   Order 4 (Wed 10-12, walking) → Luka + Petra (Ivan excluded by Order 7 conflict!)
--   Order 5 (Fri 08-11, companion) → Luka + Petra (Ivan starts 10:00 > 08:00)
--   Order 8 (Tue 08:30-12:30, escort) → Only Petra (Ana starts 09:00 > 08:30)
--   Order 9 (Mon+Thu 09-11, shop+comp) → Only Petra (others lack services/availability)
--
-- DIFFERENTTIMES SCENARIOS (partial student overlaps — tests Preskoči/Promijeni/Zamjena):
--   Order 12 (Mon+Thu 09-12, walking) → Petra=FULL, Luka=DIFFERENT(Thu 13-17 ≠ 09:00)
--   Order 13 (Mon+Thu 11-13, walking) → Petra=FULL, Luka=DIFF(Thu), Ivan=DIFF(Thu 12-17 ≠ 11:00)
--   Order 14 (Tue+Sat 09-12, companion) → Ana=DIFF(Sat 10-14 ≠ 09:00), Petra=UNAVAIL(no Sat)
--   Order 15 (Mon+Tue+Thu, companion) → Ana+Petra=FULL, Luka=DIFF(Mon green, Tue red no alt, Thu red WITH alt 13/14/15)
