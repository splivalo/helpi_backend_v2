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
- 7 Students (luka.peric, ana.matic, ivan.simic, petra.novak, marko.vukovic, maja.knezevic, dino.barisic)
- 6 Seniors with Customers (ivka.mandic, ana.horvat, josip.kovacevic, kata.babic, franjo.juric, ankica.tomic)
- 11 Orders (5 one-time + 6 recurring), 17 schedules
- 20 contact info records, 1 city (Zagreb)
- Password for all users: `Test123!`

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
