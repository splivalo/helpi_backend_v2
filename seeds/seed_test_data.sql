-- ============================================================
-- HELPI TEST DATA  Supplementary seed
-- Depends on: seed_mock_data.sql (base users/orders/jobs)
-- Adds: Reviews, PromoCodes, Notifications
-- For rollback: seed_test_data_ROLLBACK.sql
-- ============================================================
--
-- IMPORTANT: This script does NOT create ScheduleAssignments,
-- JobInstances, StudentContracts, or StudentAvailabilitySlots.
-- Those are all defined in seed_mock_data.sql.
--
-- DB state from seed_mock_data.sql:
--   Order 1 (Completed): Ana (102) -> Senior 1, Job 29 (Completed)
--   Order 3 (Completed): Petra (104) -> Senior 1, Job 30 (Completed)
--   Order 4 (Completed): Luka (101) -> Senior 3, Job 31 (Completed)
--   Order 5 (FullAssigned): Luka (101) -> Senior 6, Job 32 (Upcoming)
--   Order 6 (FullAssigned): Ana (102) -> Senior 2, Jobs 1-6 (all Upcoming)
--   Order 7 (FullAssigned): Ivan (103) -> Senior 3, Jobs 7-8 (Completed), 9-12 (Upcoming)
--   Order 8 (FullAssigned): Petra (104) -> Senior 4, Jobs 33-39 (all Upcoming)
--   Order 9 (FullAssigned): Petra (104) -> Senior 1, Jobs 40-47 (all Upcoming)
--   Order 10 (Completed):   Petra (104) -> Senior 2, Jobs 13-28 (all Completed)
--   Orders 2, 12-15: Pending (no assignments)
--   Order 11: Cancelled

BEGIN;

-- ============================================
-- 1. REVIEWS (for completed sessions)
-- ============================================
-- ReviewType: 0=SeniorToStudent, 1=StudentToSenior
-- IsPending=true -> user MUST rate (pending review prompt)
--
-- Completed sessions available: JI 7,8 (Order 7) and JI 13-28 (Order 10)

INSERT INTO "Reviews" ("Id", "Type", "SeniorId", "SeniorFullName", "StudentId", "StudentFullName", "JobInstanceId", "Rating", "Comment", "RetryCount", "MaxRetry", "NextRetryAt", "IsPending", "CreatedAt")
VALUES
-- == Order 7, Session 7 (2026-03-18, Josip Kovacevic -> Ivan Simic) ==
-- Senior 3 -> Student 103 (submitted, rating 4)
(1, 0, 3, 'Josip Kovacevic', 103, 'Ivan Simic', 7, 4.0, 'Ivan je bio jako dobar, samo malo kasni.', 0, 2, '2026-03-25 10:00:00', false, '2026-03-18 14:00:00'),
-- Student 103 -> Senior 3 (PENDING  student treba ocijeniti)
(2, 1, 3, 'Josip Kovacevic', 103, 'Ivan Simic', 7, 0, NULL, 0, 2, '2026-03-25 10:00:00', true, '2026-03-18 14:00:00'),

-- == Order 7, Session 8 (2026-03-20, Josip Kovacevic -> Ivan Simic) ==
-- Senior 3 -> Student 103 (submitted, rating 5)
(3, 0, 3, 'Josip Kovacevic', 103, 'Ivan Simic', 8, 5.0, 'Odlicno! Sve obavljeno bez primjedbi.', 0, 2, '2026-03-27 10:00:00', false, '2026-03-20 14:00:00'),
-- Student 103 -> Senior 3 (submitted, rating 5)
(4, 1, 3, 'Josip Kovacevic', 103, 'Ivan Simic', 8, 5.0, 'Gospodin Josip je jako ugodan za druzenje.', 0, 2, '2026-03-27 10:00:00', false, '2026-03-20 15:00:00'),

-- == Order 10, Session 13 (2026-01-06, Marija Horvat -> Petra Novak) ==
-- Senior 2 -> Student 104 (submitted, rating 5)
(5, 0, 2, 'Marija Horvat', 104, 'Petra Novak', 13, 5.0, 'Petra je divna! Uvijek na vrijeme i veoma ljubazna.', 0, 2, '2026-01-13 10:00:00', false, '2026-01-06 14:00:00'),
-- Student 104 -> Senior 2 (submitted, rating 5)
(6, 1, 2, 'Marija Horvat', 104, 'Petra Novak', 13, 5.0, 'Gospoda Marija je jako draga i susretljiva.', 0, 2, '2026-01-13 10:00:00', false, '2026-01-06 15:00:00'),

-- == Order 10, Session 27 (2026-02-24, Marija Horvat -> Petra Novak) ==
-- Senior 2 -> Student 104 (PENDING  senior treba ocijeniti)
(7, 0, 2, 'Marija Horvat', 104, 'Petra Novak', 27, 0, NULL, 0, 2, '2026-03-03 10:00:00', true, '2026-02-24 14:00:00'),
-- Student 104 -> Senior 2 (submitted, rating 4)
(8, 1, 2, 'Marija Horvat', 104, 'Petra Novak', 27, 4.0, 'Dobra suradnja kao i uvijek.', 0, 2, '2026-03-03 10:00:00', false, '2026-02-24 15:00:00');

-- ============================================
-- 2. PROMO CODES
-- ============================================
-- PromoCodeType: 0=Percentage, 1=FixedAmount

INSERT INTO "PromoCodes" ("Id", "Code", "Type", "DiscountValue", "MaxUses", "CurrentUses", "ValidFrom", "ValidUntil", "IsActive", "CreatedAt")
VALUES
(1, 'HELPI20', 0, 20.00, 100, 0, '2026-01-01', '2026-12-31', true, '2026-01-01 00:00:00'),
(2, 'POPUST10', 1, 10.00, 50, 0, '2026-01-01', '2026-12-31', true, '2026-01-01 00:00:00'),
(3, 'ISTEKAO99', 0, 99.00, 10, 0, '2025-01-01', '2025-12-31', true, '2025-01-01 00:00:00'),
(4, 'NEAKTIVAN', 0, 50.00, NULL, 0, '2026-01-01', '2026-12-31', false, '2026-01-01 00:00:00');

-- ============================================
-- 3. NOTIFICATIONS
-- ============================================

INSERT INTO "HNotifications" ("Id", "RecieverUserId", "Title", "Body", "TranslationKey", "Type", "IsRead", "CreatedAt", "StudentId", "SeniorId", "OrderId", "JobInstanceId")
VALUES
-- == STUDENT 103 (Ivan)  assigned to Order 7 ==
(1, 103, 'Novi posao dodijeljen', 'Dodijeljeni ste na narudzbu #7 za Josipa Kovacevica', 'job_assigned', 4, false, '2026-03-12 10:00:00', 103, 3, 7, NULL),
(2, 103, 'Posao zavrsen', 'Sesija #7 s Josipom Kovacevicem je zavrsena', 'job_completed', 7, true, '2026-03-18 14:05:00', 103, 3, 7, 7),
(3, 103, 'Ocijenite seniora', 'Molimo ocijenite vasu sesiju s Josipom Kovacevicem', 'review_request', 21, false, '2026-03-18 14:10:00', 103, 3, 7, 7),

-- == STUDENT 102 (Ana)  assigned to Order 6 ==
(4, 102, 'Novi posao dodijeljen', 'Dodijeljeni ste na narudzbu #6 za Mariju Horvat', 'job_assigned', 4, false, '2026-03-10 09:00:00', 102, 2, 6, NULL),
(5, 102, 'Podsjetnik: Posao sutra', 'Imate zakazan termin sutra u 09:00 kod Marije Horvat', 'job_reminder', 5, false, '2026-03-18 18:00:00', 102, 2, 6, 1),

-- == STUDENT 104 (Petra)  completed Order 10 ==
(6, 104, 'Ugovor istice', 'Vas ugovor HLP-2026-02 istice za 2 dana', 'contract_expiring', 15, false, '2026-03-20 08:00:00', 104, NULL, NULL, NULL),

-- == CUSTOMER 202 (Ana Horvat  orderer for Senior 2/Marija) ==
(7, 202, 'Narudzba potvrdena', 'Vasa narudzba #6 je potvrdena i dodijeljena studentu', 'order_confirmed', 0, false, '2026-03-10 09:05:00', NULL, 2, 6, NULL),
(8, 202, 'Narudzba zavrsena', 'Sve sesije za narudzbu #10 su uspjesno zavrsene', 'order_completed', 7, true, '2026-02-28 18:05:00', 104, 2, 10, NULL),

-- == CUSTOMER 203 (Josip Kovacevic) ==
(9, 203, 'Narudzba potvrdena', 'Vasa narudzba #7 je potvrdena i dodijeljena studentu', 'order_confirmed', 0, false, '2026-03-12 10:05:00', NULL, 3, 7, NULL),
(10, 203, 'Posao zavrsen', 'Sesija #7 s Ivanom Simicem je zavrsena', 'job_completed', 7, true, '2026-03-18 14:05:00', 103, 3, 7, 7),
(11, 203, 'Ocijenite studenta', 'Molimo ocijenite studenta Ivana Simica nakon sesije', 'review_request', 21, false, '2026-03-18 14:10:00', 103, 3, 7, 7),

-- == CUSTOMER 201 (Ivka Mandic) ==
(12, 201, 'Nova narudzba kreirana', 'Vasa narudzba #1 je zaprimljena i ceka obradu', 'order_created', 0, true, '2026-03-01 10:35:00', NULL, 1, 1, NULL),

-- == ADMIN-type notifications ==
(13, 201, 'Novi student registriran', 'Student Dino Barisic se registrirao', 'new_student', 24, false, '2026-01-05 10:00:00', 107, NULL, NULL, NULL),
(14, 201, 'Ugovor studenta istice', 'Ugovor studentice Petre Novak istice za 2 dana', 'contract_expiring', 15, false, '2026-03-20 08:00:00', 104, NULL, NULL, NULL);

-- ============================================
-- 4. RESET SEQUENCES
-- ============================================

SELECT setval('"Reviews_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "Reviews"));
SELECT setval('"PromoCodes_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "PromoCodes"));
SELECT setval('"HNotifications_Id_seq"', (SELECT COALESCE(MAX("Id"), 0) FROM "HNotifications"));

COMMIT;

-- ============================================================
-- SUMMARY
-- ============================================================
-- 8 reviews (5 submitted + 3 pending, for Orders 7 and 10)
-- 4 promo codes (2 active, 1 expired, 1 deactivated)
-- 14 notifications (students, customers, admin)
--
-- Does NOT modify: ScheduleAssignments, JobInstances,
-- StudentContracts, StudentAvailabilitySlots, Orders, Students
-- (all managed by seed_mock_data.sql)
