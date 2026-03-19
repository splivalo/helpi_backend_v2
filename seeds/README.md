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
- 6 Seniors with Customers
- 11 Orders (7 Pending, 2 FullAssigned, 1 Completed, 1 Cancelled)
- 16 schedules, 6 schedule assignments
- Student services, availability slots with realistic partial coverage
- Password for all users: `Test123!`

**Testing Scenarios Built-In:**

| Order | Day/Time | Service | Expected Available Students |
|-------|----------|---------|---------------------------|
| 1 | Thu 10-12 | Shopping | Ana + Petra |
| 2 | Sun 09-12 | Escort | **Nobody** (Sunday test) |
| 3 | Tue 14-17 | Shopping | Only Petra |
| 4 | Wed 10-12 | Walking | Luka + Petra (Ivan excluded - already assigned) |
| 5 | Fri 08-11 | Companionship | Luka + Petra |
| 8 | Tue 08:30-12:30 | Escort | Only Petra |
| 9 | Mon+Thu 09-11 | Shopping+Comp | Petra both; Ana Thu only; Luka Mon only |

**Student Status Coverage:**

| ID  | Name  | Status                | Notes                      |
|-----|-------|-----------------------|----------------------------|
| 101 | Luka  | Active (1)            | Mon/Wed/Fri 08-14          |
| 102 | Ana   | Active (1)            | Tue/Thu 09-15, Sat 10-14   |
| 103 | Ivan  | Active (1)            | Mon/Wed/Fri 10-17          |
| 104 | Petra | AboutToExpire (2)     | Mon-Fri 08-18 (most flexible) |
| 105 | Marko | InActive (0)          | No availability            |
| 106 | Maja  | AccountDeactivated (4)| IsSuspended=true           |
| 107 | Dino  | Expired (3)           | Tue/Thu 09-13 (not eligible) |

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
