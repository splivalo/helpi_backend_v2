-- ╔════════════════════════════════════════════════════════════════╗
-- ║  ⚠️  TODO: OBRISATI NAKON TESTIRANJA! ⚠️                    ║
-- ║  Ovo su TESTNI podaci za developer testiranje.               ║
-- ║  NE SMIJU ici u produkciju!                                   ║
-- ║  Za brisanje pokreni: seed_test_data_ROLLBACK.sql            ║
-- ╚════════════════════════════════════════════════════════════════╝

-- Ovisi na: seed_mock_data.sql (studenti 101-107, seniori 1-6, narudzbe 1-11, schedules 1-17)

BEGIN;

-- ============================================
-- 1. STUDENT CONTRACTS (aktivni ugovori za 4 studenta)
-- ============================================
-- Status je COMPUTED (ne sprema se) — ovisi o EffectiveDate/ExpirationDate vs danas
-- Studenti 101,102,103,104 dobivaju aktivne ugovore (datum u buducnosti)
-- Studenti 105,106,107 ostaju bez ugovora (InActive)

INSERT INTO "StudentContracts" ("Id", "StudentId", "ContractNumber", "CloudPath", "EffectiveDate", "ExpirationDate", "UploadedAt")
VALUES
(1, 101, 'HLP-2026-1001', '/test/contracts/hlp-2026-1001.pdf', '2026-01-01', '2027-01-01', '2026-01-01 10:00:00'),
(2, 102, 'HLP-2026-1002', '/test/contracts/hlp-2026-1002.pdf', '2026-01-15', '2027-01-15', '2026-01-15 10:00:00'),
(3, 103, 'HLP-2026-1003', '/test/contracts/hlp-2026-1003.pdf', '2026-02-01', '2027-02-01', '2026-02-01 10:00:00'),
(4, 104, 'HLP-2026-1004', '/test/contracts/hlp-2026-1004.pdf', '2026-03-01', '2027-03-01', '2026-03-01 10:00:00');

-- Aktiviraj studente koji imaju ugovor (Status=1 Active)
UPDATE "Students" SET "Status" = 1 WHERE "UserId" IN (101, 102, 103, 104);

-- ============================================
-- 2. SCHEDULE ASSIGNMENTS (dodijeli studente narudzbama)
-- ============================================
-- AssignmentStatus: 0=Accepted, 1=Declined, 2=Terminated, 3=Completed
-- Dodjela: Student 101 → OrderSchedule 1,6,12 | Student 102 → OrderSchedule 2,7,8
--          Student 103 → OrderSchedule 3,11,14 | Student 104 → OrderSchedule 4,9,16

INSERT INTO "ScheduleAssignments" ("Id", "OrderScheduleId", "OrderId", "StudentId", "Status", "IsJobInstanceSub", "AssignedAt", "AcceptedAt")
VALUES
-- Student 101 (Luka) → Order 1 (schedule 1), Order 6 (schedule 6), Order 9 (schedule 12)
(1, 1, 1, 101, 0, false, '2026-03-10 09:00:00', '2026-03-10 09:00:00'),
(2, 6, 6, 101, 0, false, '2026-03-10 09:30:00', '2026-03-10 09:30:00'),
(3, 12, 9, 101, 0, false, '2026-03-10 10:00:00', '2026-03-10 10:00:00'),

-- Student 102 (Ana) → Order 2 (schedule 2), Order 6 (schedule 7), Order 7 (schedule 8)
(4, 2, 2, 102, 0, false, '2026-03-10 11:00:00', '2026-03-10 11:00:00'),
(5, 7, 6, 102, 0, false, '2026-03-10 11:30:00', '2026-03-10 11:30:00'),
(6, 8, 7, 102, 0, false, '2026-03-10 12:00:00', '2026-03-10 12:00:00'),

-- Student 103 (Ivan) → Order 3 (schedule 3), Order 8 (schedule 11), Order 10 (schedule 14)
(7, 3, 3, 103, 0, false, '2026-03-11 09:00:00', '2026-03-11 09:00:00'),
(8, 11, 8, 103, 0, false, '2026-03-11 09:30:00', '2026-03-11 09:30:00'),
(9, 14, 10, 103, 0, false, '2026-03-11 10:00:00', '2026-03-11 10:00:00'),

-- Student 104 (Petra) → Order 4 (schedule 4), Order 7 (schedule 9), Order 11 (schedule 16)
(10, 4, 4, 104, 0, false, '2026-03-11 11:00:00', '2026-03-11 11:00:00'),
(11, 9, 7, 104, 0, false, '2026-03-11 11:30:00', '2026-03-11 11:30:00'),
(12, 16, 11, 104, 0, false, '2026-03-11 12:00:00', '2026-03-11 12:00:00');

-- Orders ostaju u Pending (Status=1) — admin ce rucno dodjeljivati kroz app

-- ============================================
-- 3. JOB INSTANCES (konkretne sesije)
-- ============================================
-- JobInstanceStatus: 0=Upcoming, 1=InProgress, 2=Completed, 3=Cancelled
-- PaymentStatus: 0=Pending, 2=Paid
-- Pricing: 14 €/h, Company 40%, Student 60%

-- == COMPLETED SESSIONS (prosle) ==
INSERT INTO "JobInstances" ("Id", "SeniorId", "CustomerId", "OrderId", "OrderScheduleId", "ContractId", "ScheduleAssignmentId", "ScheduledDate", "StartTime", "EndTime", "Status", "HourlyRate", "CompanyPercentage", "ServiceProviderPercentage", "PaymentStatus", "NeedsSubstitute", "IsRescheduleVariant")
VALUES
-- Student 101, Order 1 (jednokratna, vec prosla)
(1, 1, 201, 1, 1, 1, 1, '2026-03-10', '10:00:00', '12:00:00', 2, 14.00, 40, 60, 2, false, false),
-- Student 102, Order 2 (prosla)
(2, 3, 203, 2, 2, 2, 4, '2026-03-12', '09:00:00', '12:00:00', 2, 14.00, 40, 60, 2, false, false),
-- Student 103, Order 3 (prosla)
(3, 1, 201, 3, 3, 3, 7, '2026-03-14', '10:00:00', '12:00:00', 2, 14.00, 40, 60, 2, false, false),

-- == UPCOMING SESSIONS (buduce — za testiranje cancel/review) ==
-- Student 101, Order 9 recurring (buduci ponedjeljak)
(4, 1, 201, 9, 12, 1, 3, '2026-03-23', '09:00:00', '11:00:00', 0, 14.00, 40, 60, 0, false, false),
-- Student 101, Order 6 recurring (buduci ponedjeljak)
(5, 2, 202, 6, 6, 1, 2, '2026-03-23', '09:00:00', '12:00:00', 0, 14.00, 40, 60, 0, false, false),
-- Student 102, Order 7 recurring (buduci cetvrtak)
(6, 3, 203, 7, 8, 2, 6, '2026-03-26', '10:00:00', '12:00:00', 0, 14.00, 40, 60, 0, false, false),
-- Student 103, Order 8 recurring (buduci utorak)
(7, 4, 204, 8, 11, 3, 8, '2026-03-24', '08:30:00', '12:30:00', 0, 14.00, 40, 60, 0, false, false),
-- Student 104, Order 11 recurring (buduci utorak)
(8, 4, 204, 11, 16, 4, 12, '2026-03-24', '11:00:00', '13:00:00', 0, 14.00, 40, 60, 0, false, false),

-- == CANCELLED SESSION (za prikaz statusa) ==
(9, 3, 203, 4, 4, NULL, 10, '2026-03-18', '10:00:00', '12:00:00', 3, 14.00, 40, 60, 0, false, false),

-- == Dodatne upcoming za bogat raspored ==
-- Student 102, Order 6 recurring (cetvrtak)
(10, 2, 202, 6, 7, 2, 5, '2026-03-26', '09:00:00', '12:00:00', 0, 14.00, 40, 60, 0, false, false),
-- Student 103, Order 10 recurring (utorak)
(11, 2, 202, 10, 14, 3, 9, '2026-03-24', '10:00:00', '13:00:00', 0, 14.00, 40, 60, 0, false, false),
-- Student 104, Order 7 recurring (petak)
(12, 3, 203, 7, 9, 4, 11, '2026-03-27', '10:00:00', '12:00:00', 0, 14.00, 40, 60, 0, false, false);

-- ============================================
-- 4. REVIEWS (za completed sesije — mix pending i submitted)
-- ============================================
-- ReviewType: 0=SeniorToStudent, 1=StudentToSenior
-- IsPending=true znaci da korisnik MORA ocijeniti (pending review prompt)

INSERT INTO "Reviews" ("Id", "Type", "SeniorId", "SeniorFullName", "StudentId", "StudentFullName", "JobInstanceId", "Rating", "Comment", "RetryCount", "MaxRetry", "NextRetryAt", "IsPending", "CreatedAt")
VALUES
-- Session 1: Senior 1 → Student 101 (submitted, rating 5)
(1, 0, 1, 'Ivka Mandic', 101, 'Luka Peric', 1, 5.0, 'Luka je odlican, uvijek na vrijeme!', 0, 2, '2026-03-10 14:00:00', false, '2026-03-10 14:00:00'),
-- Session 1: Student 101 → Senior 1 (PENDING — student treba ocijeniti)
(2, 1, 1, 'Ivka Mandic', 101, 'Luka Peric', 1, 0, NULL, 0, 2, '2026-03-17 14:00:00', true, '2026-03-10 14:00:00'),

-- Session 2: Senior 3 → Student 102 (submitted, rating 4)
(3, 0, 3, 'Josip Kovacevic', 102, 'Ana Matic', 2, 4.0, 'Ana je bila draga i ljubazna.', 0, 2, '2026-03-12 14:00:00', false, '2026-03-12 14:00:00'),
-- Session 2: Student 102 → Senior 3 (submitted, rating 5)
(4, 1, 3, 'Josip Kovacevic', 102, 'Ana Matic', 2, 5.0, 'Gospodin Josip je super!', 0, 2, '2026-03-12 15:00:00', false, '2026-03-12 15:00:00'),

-- Session 3: Senior 1 → Student 103 (PENDING — senior treba ocijeniti)
(5, 0, 1, 'Ivka Mandic', 103, 'Ivan Simic', 3, 0, NULL, 0, 2, '2026-03-17 10:00:00', true, '2026-03-14 12:00:00'),
-- Session 3: Student 103 → Senior 1 (PENDING — student treba ocijeniti)
(6, 1, 1, 'Ivka Mandic', 103, 'Ivan Simic', 3, 0, NULL, 0, 2, '2026-03-17 10:00:00', true, '2026-03-14 12:00:00');

-- ============================================
-- 5. PROMO CODES (za testiranje promo flow-a)
-- ============================================
-- PromoCodeType: 0=Percentage, 1=FixedAmount

INSERT INTO "PromoCodes" ("Id", "Code", "Type", "DiscountValue", "MaxUses", "CurrentUses", "ValidFrom", "ValidUntil", "IsActive", "CreatedAt")
VALUES
(1, 'HELPI20', 0, 20.00, 100, 0, '2026-01-01', '2026-12-31', true, '2026-01-01 00:00:00'),
(2, 'POPUST10', 1, 10.00, 50, 0, '2026-01-01', '2026-12-31', true, '2026-01-01 00:00:00'),
(3, 'ISTEKAO99', 0, 99.00, 10, 0, '2025-01-01', '2025-12-31', true, '2025-01-01 00:00:00'),
(4, 'NEAKTIVAN', 0, 50.00, NULL, 0, '2026-01-01', '2026-12-31', false, '2026-01-01 00:00:00');

-- ============================================
-- 6. NOTIFICATIONS (testne za sve tipove korisnika)
-- ============================================
-- NotificationType enum: see entity definition
-- RecieverUserId = AspNetUsers.Id

INSERT INTO "HNotifications" ("Id", "RecieverUserId", "Title", "Body", "TranslationKey", "Type", "IsRead", "CreatedAt", "StudentId", "SeniorId", "OrderId", "JobInstanceId")
VALUES
-- == STUDENT 101 (Luka) notifications ==
(1, 101, 'Novi posao dodijeljen', 'Dodijeljeni ste na narudzbu #1 za Ivku Mandic', 'job_assigned', 4, false, '2026-03-10 09:00:00', 101, 1, 1, NULL),
(2, 101, 'Podsjetnik: Posao sutra', 'Imate zakazan termin sutra u 09:00 kod Ivke Mandic', 'job_reminder', 5, false, '2026-03-22 18:00:00', 101, 1, 9, 4),
(3, 101, 'Ugovor aktivan', 'Vas ugovor HLP-2026-1001 je sada aktivan', 'contract_active', 15, true, '2026-01-01 10:00:00', 101, NULL, NULL, NULL),
(4, 101, 'Ocijenite seniora', 'Molimo ocijenite vasu sesiju s Ivkom Mandic', 'review_request', 21, false, '2026-03-10 15:00:00', 101, 1, 1, 1),

-- == STUDENT 102 (Ana) notifications ==
(5, 102, 'Novi posao dodijeljen', 'Dodijeljeni ste na narudzbu #2 za Josipa Kovacevica', 'job_assigned', 4, false, '2026-03-10 11:00:00', 102, 3, 2, NULL),
(6, 102, 'Posao zavrsen', 'Sesija s Josipom Kovacevicem je zavrsena', 'job_completed', 7, true, '2026-03-12 14:00:00', 102, 3, 2, 2),

-- == STUDENT 103 (Ivan) notifications ==
(7, 103, 'Novi posao dodijeljen', 'Dodijeljeni ste na narudzbu #3 za Ivku Mandic', 'job_assigned', 4, false, '2026-03-11 09:00:00', 103, 1, 3, NULL),
(8, 103, 'Ocijenite seniora', 'Molimo ocijenite vasu sesiju s Ivkom Mandic', 'review_request', 21, false, '2026-03-14 13:00:00', 103, 1, 3, 3),

-- == CUSTOMER 201 (Ivka — senior 1 owner) notifications ==
(9, 201, 'Narudzba potvrena', 'Vasa narudzba #1 je potvrena i dodijeljena studentu', 'order_confirmed', 0, false, '2026-03-10 09:05:00', NULL, 1, 1, NULL),
(10, 201, 'Posao zavrsen', 'Sesija #1 s Lukom Pericem je zavrsena', 'job_completed', 7, false, '2026-03-10 12:05:00', 101, 1, 1, 1),
(11, 201, 'Placanje uspjesno', 'Placanje od 28.00 EUR je procesirano za sesiju #1', 'payment_success', 1, true, '2026-03-10 12:10:00', NULL, 1, 1, 1),
(12, 201, 'Ocijenite studenta', 'Molimo ocijenite studenta Ivana Simica nakon sesije', 'review_request', 21, false, '2026-03-14 12:05:00', 103, 1, 3, 3),

-- == CUSTOMER 203 (Josip — senior 3 owner) notifications ==
(13, 203, 'Narudzba potvrena', 'Vasa narudzba #2 je potvrena', 'order_confirmed', 0, false, '2026-03-10 11:05:00', NULL, 3, 2, NULL),
(14, 203, 'Posao zavrsen', 'Sesija #2 s Anom Matic je zavrsena', 'job_completed', 7, true, '2026-03-12 14:05:00', 102, 3, 2, 2),
(15, 203, 'Posao otkazan', 'Sesija za narudzbu #4 je otkazana', 'job_cancelled', 8, true, '2026-03-18 10:05:00', NULL, 3, 4, 9),

-- == CUSTOMER 204 (Kata — senior 4 owner) notifications ==
(16, 204, 'Narudzba potvrena', 'Vasa narudzba #8 je potvrena i dodijeljena studentu', 'order_confirmed', 0, false, '2026-03-11 09:35:00', NULL, 4, 8, NULL),
(17, 204, 'Podsjetnik: Posao sutra', 'Imate zakazan termin sutra', 'job_reminder', 5, false, '2026-03-23 18:00:00', 103, 4, 8, 7),

-- == ADMIN notifications (User 1 — pretpostavljam admin user) ==
-- Provjeri postoji li admin user s ID=1, inace skip ovu sekciju
(18, 201, 'Novi student registriran', 'Student Dino Barisic se registrirao', 'new_student', 24, false, '2026-01-05 10:00:00', 107, NULL, NULL, NULL),
(19, 201, 'Novi senior registriran', 'Senior Ankica Tomic je registrirana', 'new_senior', 25, false, '2026-02-20 10:00:00', NULL, 6, NULL, NULL);

-- ============================================
-- 7. STUDENT AVAILABILITY (dostupnost za testiranje assign flow)
-- ============================================
-- Student 101: pon-pet 08-16
-- Student 102: pon,sri,pet 09-15
-- Student 103: uto,cet 08-17, sub 09-13
-- Student 104: pon-pet 10-18

INSERT INTO "StudentAvailabilitySlots" ("StudentId", "DayOfWeek", "StartTime", "EndTime")
VALUES
-- Student 101 (Luka) — pon-pet 08-16
(101, 1, '08:00:00', '16:00:00'),
(101, 2, '08:00:00', '16:00:00'),
(101, 3, '08:00:00', '16:00:00'),
(101, 4, '08:00:00', '16:00:00'),
(101, 5, '08:00:00', '16:00:00'),

-- Student 102 (Ana) — pon, sri, pet 09-15
(102, 1, '09:00:00', '15:00:00'),
(102, 3, '09:00:00', '15:00:00'),
(102, 5, '09:00:00', '15:00:00'),

-- Student 103 (Ivan) — uto, cet 08-17, sub 09-13
(103, 2, '08:00:00', '17:00:00'),
(103, 4, '08:00:00', '17:00:00'),
(103, 6, '09:00:00', '13:00:00'),

-- Student 104 (Petra) — pon-pet 10-18
(104, 1, '10:00:00', '18:00:00'),
(104, 2, '10:00:00', '18:00:00'),
(104, 3, '10:00:00', '18:00:00'),
(104, 4, '10:00:00', '18:00:00'),
(104, 5, '10:00:00', '18:00:00');

-- ============================================
-- 8. RESET SEQUENCES
-- ============================================

SELECT setval('"StudentContracts_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "StudentContracts"));
SELECT setval('"ScheduleAssignments_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "ScheduleAssignments"));
SELECT setval('"JobInstances_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "JobInstances"));
SELECT setval('"Reviews_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "Reviews"));
SELECT setval('"PromoCodes_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "PromoCodes"));
SELECT setval('"HNotifications_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "HNotifications"));
-- StudentAvailabilitySlots nema Id — composite key (StudentId, DayOfWeek), nema sequence

COMMIT;

-- ╔════════════════════════════════════════════════════════════════╗
-- ║  SUMMARY                                                      ║
-- ╠════════════════════════════════════════════════════════════════╣
-- ║  4 student contracts (101-104 aktivni)                        ║
-- ║  12 schedule assignments (studenti dodijeljeni narudzbama)    ║
-- ║  12 job instances (3 completed, 8 upcoming, 1 cancelled)      ║
-- ║  6 reviews (2 submitted, 4 pending)                           ║
-- ║  4 promo codes (2 aktivna, 1 istekao, 1 neaktivan)           ║
-- ║  19 notifications (mix read/unread za studente i seniore)     ║
-- ║  16 availability slots (4 studenta, composite PK bez Id)      ║
-- ╠════════════════════════════════════════════════════════════════╣
-- ║  PROMO KODOVI ZA TESTIRANJE:                                  ║
-- ║  HELPI20  → 20% popust (aktivan)                             ║
-- ║  POPUST10 → 10 EUR popust (aktivan)                           ║
-- ║  ISTEKAO99 → istekao (prosle godine)                          ║
-- ║  NEAKTIVAN → deaktiviran                                      ║
-- ╠════════════════════════════════════════════════════════════════╣
-- ║  LOGIN PODACI ZA TESTIRANJE:                                  ║
-- ║  Student: luka.peric@email.com (ID 101, ima ugovor+sesije)   ║
-- ║  Student: ana.matic@email.com (ID 102, ima ugovor+sesije)    ║
-- ║  Senior:  ivka.mandic@email.com (ID 201, ima narudzbe)       ║
-- ║  Senior:  josip.kovacevic@email.com (ID 203, ima narudzbe)   ║
-- ╚════════════════════════════════════════════════════════════════╝
