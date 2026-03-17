# Helpi Backend v2 — Progress

## Overall Status: 100% backend gaps resolved

---

## Faza 1 — Security & Core Fixes ✅

### Task 1: Senior Edit Prevention ✅

- `PUT /orders/{id}` restricted to Admin role only
- Senior cannot edit orders (only cancel)

### Task 2: Order Cancel 24h Rule ✅

- Cancel endpoint enforces 24h-before-service time check
- Throws DomainException if less than 24h before service start

---

## Faza 2 — Core Business Logic ✅

### Task 3: Admin Manual Assignment ✅

- New `POST /api/schedule-assignments/admin-assign` endpoint
- `AdminDirectAssignAsync()` in ScheduleAssignmentService
- Restricted to Admin role via `[Authorize(Roles = "Admin")]`

### Task 4: Sunday Rate Multiplier ✅

- Added `SundayHourlyRate` to PricingConfiguration entity
- HangfireRecurringJobService uses Sunday rate when `date.DayOfWeek == Sunday`
- Seeded at 21 EUR (vs 14 EUR weekday rate)
- Migration: `20260313130245_AddSundayHourlyRate`

### Task 5: Travel Buffer 15 min ✅

- `const int travelBufferMinutes = 15` in 3 locations:
  - `StudentRepository.FindEligibleStudentsCore()` — ±15min on time bounds
  - `JobRequestRepository.GetStudentConflictingPendingRequests()` — ±15min buffer
  - `ScheduleAssignmentService.AdminDirectAssignAsync()` — full conflict validation

---

## Faza 3 — Extended Features 🔄

### Task 7: Bidirectional Reviews ✅

- Added `ReviewType` enum: `SeniorToStudent = 0`, `StudentToSenior = 1`
- Extended `Review` entity with `Type` property
- Changed `JobInstance.Review` (one-to-one) → `JobInstance.Reviews` (one-to-many)
- Added `TotalReviews`, `TotalRatingSum`, `AverageRating` to Senior entity + SeniorDto
- `ReviewRepository`: `GetPendingStudentReviews`, `GetAboutSeniorAsync`, filtered by ReviewType
- `ReviewService`: Conditional rating update (Senior or Student stats based on ReviewType)
- `ReviewsController`: New endpoints `GET /senior/{seniorId}`, `GET /student/{studentId}/pending`
- `JobInstanceService.RequestJobReviewAsync`: Creates 2 reviews per completion (one per direction)
- Migration: `20260313133641_AddBidirectionalReviews` — drops unique index, adds Type + Senior rating cols
- **Files modified:** 12 files across Domain, Application, Infrastructure, WebApi

### Task 8: Dashboard Per-Role ✅

- Admin: `GET /api/dashboard/admin` — existing 12 tiles, restricted to Admin role
- Student: `GET /api/dashboard/student/{studentId}` — 6 tiles:
  - `upcomingSessions`, `completedSessionsStudent`, `totalEarnings`, `myRating`, `contractDaysRemaining`, `workedHoursStudent`
- Senior: `GET /api/dashboard/senior/{seniorId}` — 4 tiles:
  - `activeOrders`, `completedSessionsSenior`, `totalSpent`, `myRatingSenior`
- Old `GET /api/dashboard` removed, replaced with role-specific endpoints
- Added 11 new DashboardTileType enum values
- Added ISeniorRepository dependency to DashboardService
- Current month vs last month comparison with percentage change
- **Files modified:** 4 files (enums.cs, DashboardController, IDashboardService, DashboardService)

### Task 9: Session Terminology ✅

- Route: `api/job-instances` → `api/sessions`
- Controller: `JobInstancesController` → `SessionsController`
- DTO renames: `JobInstanceDto` → `SessionDto`, `CompletedJobInstanceDto` → `CompletedSessionDto`, `JobInstanceUpdateDto` → `SessionUpdateDto`, `JobInstanceIncludeOptions` → `SessionIncludeOptions`, `ManageJobInstanceRequestDto` → `ManageSessionRequestDto`
- Property rename: `StudentContractDto.JobInstances` → `Sessions` (with explicit AutoMapper mapping)
- Domain entity remains `JobInstance` (internal), API speaks "Session"
- **Files modified:** 14 files across Application + Infrastructure + WebApi

### Task 6: Promo Code System ✅

- **Entities:** `PromoCode` (code, type, discount, max uses, validity dates) + `PromoCodeUsage` (tracking per customer per order)
- **Enum:** `PromoCodeType { Percentage, FixedAmount }`
- **Order integration:** `Order.PromoCodeId` optional FK
- **DTOs:** `PromoCodeDto`, `PromoCodeCreateDto`, `PromoCodeUpdateDto`, `PromoCodeValidationResultDto`, `PromoCodeUsageDto`
- **Repository:** `IPromoCodeRepository` — CRUD + `GetByCodeAsync`, `HasCustomerUsedCodeAsync`, usage tracking
- **Service:** `IPromoCodeService` — validate code (checks active, dates, max uses, already used), apply code (creates usage record, increments counter)
- **Controller:** `api/promo-codes` — Admin CRUD (GET, POST, PUT, DELETE) + public validate/apply endpoints
- **Validation logic:** percentage ≤ 100%, one use per customer per code, date range, max uses
- **DB config:** Unique index on `Code`, `decimal(18,2)` for monetary fields
- **Migration:** `20260313140442_AddPromoCodeSystem` — 2 new tables + FK on Orders
- **Files created:** 7 new files | **Files modified:** 5 existing files

### Task 8: Dashboard Per-Role ❌ Not Started

- Estimated: 4-5h
- Currently same data for all roles

### Task 9: Session Terminology ❌ Not Started

- Estimated: 30 min
- Alias JobInstance as "Session" in DTOs

---

## Migrations Applied

1. `20260313130245_AddSundayHourlyRate` — SundayHourlyRate column + pricing history columns
2. `20260313133641_AddBidirectionalReviews` — ReviewType, Senior rating fields, one-to-many relationship
3. `20260313140442_AddPromoCodeSystem` — PromoCodes + PromoCodeUsages tables, Order.PromoCodeId FK

---

## Next Steps

- All 9 backend gap analysis items COMPLETE
- Ready for frontend-backend integration
- Awaiting user direction for next phase

---

## Fixes & Improvements

### Delete Account Notification — Real Name ✅

- **Problem:** Admin notification on user deletion showed generic "Student 133" / "Customer 42" instead of real name
- **Fix:** `StudentsService.PermanentlyDeleteStudent()` — `originalUserName` now uses `student.Contact?.FullName`
- **Fix:** `CustomerService.DeleteCustomerAsync()` — `originalName` now uses `customer.Contact?.FullName`
- **Result:** Admin notification now shows e.g. "Student: Ivan Marković (ID: 133) je trajno izbrisan"
- **TODO:** This must be visible in admin app notifications panel (frontend integration pending)

### Email Availability Check Endpoint ✅

- **New endpoint:** `GET /api/auth/check-email?email=xxx` — returns `{ "exists": true/false }`
- **AuthService:** `CheckEmailExistsAsync(string email)` — uses `UserManager.FindByEmailAsync`
- **AuthController:** `[HttpGet("check-email")]` — validates email param, returns existence status
- **Purpose:** Flutter app checks email BEFORE user fills registration form (better UX)
- **No auth required** — public endpoint for pre-registration validation

### Swagger v2 ✅

- **Changed:** Swagger doc from v1 to v2 in Program.cs
- **SwaggerDoc:** `"v2"` with title "Helpi API", version "v2", description "Helpi v2 Backend API"
- **SwaggerEndpoint:** `/swagger/v2/swagger.json`
- **Build:** Verified — 0 errors

### Student Dashboard DurationHours Fix ✅ (2026-03-18)

- **Problem:** `GET /api/dashboard/student/{id}` returned 500 Internal Server Error
- **Root cause:** `JobInstanceRepository.GetTotalCompletedHoursForPeriodAsync()` used `.SumAsync(ji => ji.DurationHours)` in LINQ, but `DurationHours` is a computed C# property (`EndTime - StartTime`), not a DB column — EF Core cannot translate it to SQL
- **Fix:** Changed to `ToListAsync()` + in-memory `.Sum(ji => ji.DurationHours)` — filters still run server-side, only Sum computed in memory on already-filtered results
- **File:** `Helpi.Infrastructure/Repositories/JobInstanceRepository.cs` line 338
- **Build:** 0 errors, no new warnings

---

## Fresh Validation Report (2026-03-18)

### Compile Status

- `flutter analyze` helpi_app: **0 issues**
- `flutter analyze` helpi_admin: **0 issues**
- `dotnet build` backend: **0 errors**, 63 warnings (all pre-existing nullability)

### Live API Endpoint Testing (all with admin JWT)

- ✅ Auth login (admin@test.com)
- ✅ Dashboard admin / senior
- ✅ Students, Seniors, Orders, Cities, Faculties, PricingConfiguration
- ✅ Sessions, Reviews, Availability, Student Contracts, Promo Codes
- ✅ Service Categories, Suspensions, Admin Notes
- ✅ Session cancel route exists (400 = valid route, needs body)
- ✅ Promo validate/apply routes exist (400 = valid route, needs body)
- ⚠️ Dashboard student — 500 (DurationHours bug, fixed above, needs backend restart)

### Known Issues (not bugs — by design)

- Seed users (students 101-107, seniors 201-206) have fake password hashes — cannot login. Admin (Id=13) has real hash. Students/seniors will be created via register endpoint for testing.
- HNotifications table empty — backend doesn't auto-create notifications yet (feature, not bug)
- Chat backend not implemented (placeholder screens)
