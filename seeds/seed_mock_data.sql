-- ============================================
-- HELPI MOCK DATA SEED SCRIPT
-- ============================================
-- Svi studenti bez ugovora, sve narudzbe u obradi (Pending)
-- Admin ce rucno: uploadati ugovore + dodijeliti narudzbe
-- ============================================

BEGIN;

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
-- 3. STUDENTS (bez ugovora - Status=InActive)
-- ============================================
-- StudentStatus: 0=InActive, 1=Active

INSERT INTO "Students" ("UserId", "StudentNumber", "FacultyId", "DateRegistered", "ContactId", "Status", "TotalReviews", "TotalRatingSum", "AverageRating")
VALUES
(101, '0036512345', 1, '2025-11-01', 101, 0, 12, 57.6, 4.8),
(102, '0036598765', 2, '2025-12-15', 102, 0, 8, 36.8, 4.6),
(103, '0036554321', 3, '2026-01-10', 103, 0, 5, 21.0, 4.2),
(104, '0036567890', 4, '2026-02-20', 104, 0, 3, 15.0, 5.0),
(105, '0036511111', 5, '2025-10-20', 105, 0, 7, 31.5, 4.5),
(106, '0036522222', 6, '2025-11-15', 106, 0, 10, 49.0, 4.9),
(107, '0036533333', 7, '2026-01-05', 107, 0, 6, 25.8, 4.3);

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
-- 9. ORDERS (sve Pending - cekaju dodjelu)
-- ============================================
-- OrderStatus: 0=InActive, 1=Pending, 2=FullAssigned, 3=Completed, 4=Cancelled
-- RecurrencePattern: 0=Weekly

-- Jednokratne narudzbe
INSERT INTO "Orders" ("Id", "SeniorId", "Status", "IsRecurring", "RecurrencePattern", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
VALUES
(1, 1, 1, false, NULL, '2026-03-20', '2026-03-20', 'Mlijeko i kruh iz Konzuma, lijekove iz ljekarne.', '2026-03-01 10:30:00', '2026-03-01 10:30:00'),
(2, 3, 1, false, NULL, '2026-03-22', '2026-03-22', 'Pratnja na kontrolu i druzenje nakon.', '2026-03-03 11:00:00', '2026-03-03 11:00:00'),
(3, 1, 1, false, NULL, '2026-03-25', '2026-03-25', 'Kupovina namirnica.', '2026-03-05 09:00:00', '2026-03-05 09:00:00'),
(4, 3, 1, false, NULL, '2026-04-01', '2026-04-01', 'Setnja u parku.', '2026-03-05 10:00:00', '2026-03-05 10:00:00'),
(5, 6, 1, false, NULL, '2026-04-10', '2026-04-10', 'Druzenje i razgovor.', '2026-03-05 11:00:00', '2026-03-05 11:00:00');

-- Ponavljajuce narudzbe
INSERT INTO "Orders" ("Id", "SeniorId", "Status", "IsRecurring", "RecurrencePattern", "StartDate", "EndDate", "Notes", "CreatedAt", "UpdatedAt")
VALUES
(6, 2, 1, true, 0, '2026-03-17', '2026-06-30', 'Pomoc s ciscenjem i druzenje.', '2026-03-01 14:00:00', '2026-03-01 14:00:00'),
(7, 3, 1, true, 0, '2026-03-18', '2026-06-30', 'Setnja i druzenje.', '2026-02-20 09:00:00', '2026-02-20 09:00:00'),
(8, 4, 1, true, 0, '2026-03-17', '2026-04-30', 'Pratnja do lijecnika svaki utorak.', '2026-02-15 11:30:00', '2026-02-15 11:30:00'),
(9, 1, 1, true, 0, '2026-03-17', '2026-05-31', 'Kupovina i druzenje ponedjeljkom i cetvrtkom.', '2026-02-25 10:00:00', '2026-02-25 10:00:00'),
(10, 2, 1, true, 0, '2026-03-18', '2026-05-31', 'Pomoc u kuci i setnja.', '2026-02-20 14:00:00', '2026-02-20 14:00:00'),
(11, 4, 1, true, 0, '2026-03-17', '2026-06-30', 'Pomoc s ciscenjem utorak i cetvrtak.', '2026-03-04 08:30:00', '2026-03-04 08:30:00');

-- ============================================
-- 10. ORDER SCHEDULES (rasporedi za narudzbe)
-- ============================================
-- DayOfWeek: 0=Sunday, 1=Monday, 2=Tuesday, ...

-- Jednokratne (jedna sesija)
INSERT INTO "OrderSchedules" ("Id", "OrderId", "DayOfWeek", "StartTime", "EndTime", "IsCancelled", "AutoScheduleAttemptCount", "AllowAutoScheduling")
VALUES
(1, 1, 4, '10:00:00', '12:00:00', false, 0, true),
(2, 2, 0, '09:00:00', '12:00:00', false, 0, true),
(3, 3, 2, '10:00:00', '12:00:00', false, 0, true),
(4, 4, 3, '10:00:00', '12:00:00', false, 0, true),
(5, 5, 4, '09:00:00', '12:00:00', false, 0, true);

-- Ponavljajuce (vise rasporeda)
INSERT INTO "OrderSchedules" ("Id", "OrderId", "DayOfWeek", "StartTime", "EndTime", "IsCancelled", "AutoScheduleAttemptCount", "AllowAutoScheduling")
VALUES
(6, 6, 1, '09:00:00', '12:00:00', false, 0, true),
(7, 6, 4, '09:00:00', '12:00:00', false, 0, true),
(8, 7, 3, '10:00:00', '12:00:00', false, 0, true),
(9, 7, 5, '10:00:00', '12:00:00', false, 0, true),
(10, 7, 0, '10:00:00', '12:00:00', false, 0, true),
(11, 8, 2, '08:30:00', '12:30:00', false, 0, true),
(12, 9, 1, '09:00:00', '11:00:00', false, 0, true),
(13, 9, 4, '09:00:00', '11:00:00', false, 0, true),
(14, 10, 2, '10:00:00', '13:00:00', false, 0, true),
(15, 10, 4, '10:00:00', '13:00:00', false, 0, true),
(16, 11, 2, '11:00:00', '13:00:00', false, 0, true),
(17, 11, 4, '11:00:00', '13:00:00', false, 0, true);

-- ============================================
-- 11. ORDER SERVICES (usluge na narudzbama)
-- ============================================
-- 6 servisa u sustavu (stari servisi obrisani):
--   1=Društvo (Companionship), 4=Šetnja (Walking), 11=Kupovina (Shopping),
--   21=Kućanstvo (House Help), 31=Pratnja (Escort), 41=Ostalo (Other)

INSERT INTO "OrderServices" ("OrderId", "ServiceId") VALUES
(1, 11),
(2, 31), (2, 1),
(3, 11),
(4, 4),
(5, 1),
(6, 21),
(7, 4),
(8, 31),
(9, 11), (9, 1),
(10, 4), (10, 21),
(11, 21);

-- ============================================
-- 12. RESET SEQUENCES
-- ============================================

SELECT setval('"AspNetUsers_Id_seq"', (SELECT MAX("Id") FROM "AspNetUsers"));
SELECT setval('"ContactInfos_Id_seq"', (SELECT MAX("Id") FROM "ContactInfos"));
SELECT setval('"Seniors_Id_seq"', (SELECT MAX("Id") FROM "Seniors"));
SELECT setval('"Orders_Id_seq"', (SELECT MAX("Id") FROM "Orders"));
SELECT setval('"OrderSchedules_Id_seq"', (SELECT MAX("Id") FROM "OrderSchedules"));

COMMIT;

-- ============================================
-- SUMMARY
-- ============================================
-- 7 studenata (User 101-107, Contact 101-107, Student 101-107)
-- 6 customera (User 201-206, Contact 201-206, Customer 201-206)
-- 6 seniora (Senior 1-6, Contact 301-306)
-- 11 narudzbi (Order 1-11) - sve Pending
-- 17 rasporeda (OrderSchedule 1-17)
--
-- Senior 2 (Marija) ima orderera (Ana) kroz Customer 202
--
-- SVE NARUDZBE SU PENDING - cekaju dodjelu studenta!
-- Studenti nemaju ugovore - prvo upload ugovora, pa dodjela.
