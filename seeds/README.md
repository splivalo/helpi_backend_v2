# Seeds — Demo & Test Data

> **Instructions for setting up the local database with demo data.**

## Prerequisites

- PostgreSQL running on `localhost:5432`
- Database `HelpiDB` created (EF migrations applied)
- Backend started at least once (`dotnet run`) so tables are created

## Execution Order

### 1. Base demo data

```bash
psql -U postgres -d HelpiDB -f seeds/seed_mock_data.sql
```

Creates:

- 1 Admin (`admin@test.com` / `Test123!`)
- 7 Students (mixed statuses: 3 Active, 1 AboutToExpire, 1 InActive, 1 Deactivated, 1 Expired)
- 5 Student Contracts (matching student statuses)
- 6 Seniors with Customers
- 14 Orders (9 Pending, 2 FullAssigned, 1 Completed, 1 Cancelled, 1 InActive)
- 22 schedules, 6 schedule assignments
- 15 student-service mappings, 20 availability slots
- Password for all users: `Test123!`

**Full-Match Testing Scenarios:**

| Order | Day/Time        | Service       | Expected Available Students                     |
| ----- | --------------- | ------------- | ----------------------------------------------- |
| 1     | Thu 10-12       | Shopping      | Ana + Petra                                     |
| 2     | Sun 09-12       | Escort        | **Nobody** (Sunday test)                        |
| 3     | Tue 14-17       | Shopping      | Only Petra                                      |
| 4     | Wed 10-12       | Walking       | Luka + Petra (Ivan excluded - already assigned) |
| 5     | Fri 08-11       | Companionship | Luka + Petra                                    |
| 8     | Tue 08:30-12:30 | Escort        | Only Petra                                      |
| 9     | Mon+Thu 09-11   | Shopping+Comp | Petra both; Ana Thu only; Luka Mon only         |

**DifferentTimes Testing Scenarios (for Skip/Change/Replace flows):**

| Order | Days    | Time  | Service       | Full Match | DifferentTimes                                   |
| ----- | ------- | ----- | ------------- | ---------- | ------------------------------------------------ |
| 12    | Mon+Thu | 09-12 | Walking       | Petra      | Luka (Mon only), Ana (Thu only)                  |
| 13    | Mon+Thu | 11-13 | Walking       | Petra      | Luka (Mon only), Ivan (Mon only), Ana (Thu only) |
| 14    | Tue+Sat | 09-12 | Companionship | —          | Ana (Tue only), Petra (Tue only)                 |

> **Order 14 Saturday:** Nobody available — Ana has Sat 10-14 (starts 10, not 09).

**Student Status Coverage:**

| ID  | Name  | Status                 | Contract                           | Availability                            |
| --- | ----- | ---------------------- | ---------------------------------- | --------------------------------------- |
| 101 | Luka  | Active (1)             | HLP-2025-11 → 2026-11-01           | Mon/Wed/Fri 08-14, **Thu 13-17**        |
| 102 | Ana   | Active (1)             | HLP-2025-12 → 2026-12-15           | **Mon 12-15**, Tue/Thu 09-15, Sat 10-14 |
| 103 | Ivan  | Active (1)             | HLP-2026-01 → 2027-01-10           | Mon/Wed/Fri 10-17, **Tue/Thu 12-17**    |
| 104 | Petra | AboutToExpire (2)      | HLP-2026-02 → 2026-03-22 (3 days!) | Mon-Fri 08-18                           |
| 105 | Marko | InActive (0)           | —                                  | No availability                         |
| 106 | Maja  | AccountDeactivated (4) | —                                  | IsSuspended=true                        |
| 107 | Dino  | Expired (3)            | HLP-2025-06 → expired 2025-12-01   | Tue/Thu 09-13                           |

### 2. Extended test data (sessions, reviews, promo codes)

```bash
psql -U postgres -d HelpiDB -f seeds/seed_test_data.sql
```

Adds:

- 4 student contracts (students 101-104 become active)
- 12 schedule assignments
- 12 job instances (3 completed, 8 upcoming, 1 cancelled)
- 6 reviews (2 submitted, 4 pending)
- 4 promo codes (`HELPI20`, `POPUST10`, `ISTEKAO99`, `NEAKTIVAN`)
- 19 notifications
- 16 student availability slots

### 3. Cleanup test data (optional)

```bash
psql -U postgres -d HelpiDB -f seeds/seed_test_data_ROLLBACK.sql
```

Removes everything from step 2, reverts students back to Inactive.

## Promo Codes for Testing

| Code        | Type       | Discount | Status         |
| ----------- | ---------- | -------- | -------------- |
| `HELPI20`   | Percentage | 20%      | ✅ Active      |
| `POPUST10`  | Fixed      | 10 EUR   | ✅ Active      |
| `ISTEKAO99` | Percentage | 99%      | ❌ Expired     |
| `NEAKTIVAN` | Percentage | 50%      | ❌ Deactivated |

## Login Credentials

| Role    | Email                     | Password |
| ------- | ------------------------- | -------- |
| Admin   | admin@test.com            | Test123! |
| Student | luka.peric@email.com      | Test123! |
| Student | ana.matic@email.com       | Test123! |
| Senior  | ivka.mandic@email.com     | Test123! |
| Senior  | josip.kovacevic@email.com | Test123! |
