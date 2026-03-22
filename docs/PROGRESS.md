# Helpi Backend v2 — Progress

> Zadnja izmjena: 2026-03-22

## 📖 Za Sidney-a — Što čitati (sva 3 repoa)

| GitHub repo (splivalo/) | Fajl                             | Sadržaj                                          |
| ----------------------- | -------------------------------- | ------------------------------------------------ |
| **helpi_administrator** | **docs/ROADMAP.md**              | **Svi preostali TODO-ovi (START HERE)**          |
| helpi_administrator     | docs/PROGRESS.md                 | Admin app status (98% frontend done)             |
| helpi_administrator     | docs/ARCHITECTURE.md             | Admin tech stack, folder structure, UI standardi |
| helpi_administrator     | docs/PROJECT_HISTORY.md          | Kronologija odluka (Feb→Mart 2026)               |
| **helpi_backend_v2**    | **docs/PROGRESS.md (ovaj fajl)** | Backend task tracking (16 taskova ✅)            |
| helpi_backend_v2        | README.md                        | DB schema, use case flows, 19 LINQ queries       |
| helpi_backend_v2        | seeds/README.md                  | Test data, login credentials, promo codes        |
| helpi_apps              | README.md                        | App tech stack, Riverpod/SignalR info            |
| helpi_apps              | docs/ARCHITECTURE.md             | Folder structure, 64 fajlova, providers          |

---

## Overall Status: 100% backend gaps resolved + suspension + holidays

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

## Faza 3 — Extended Features ✅

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

---

## Faza 4 — Admin Assign & Distance Improvements ✅

### Task 10: OrderDto Senior Lat/Lng ✅ (2026-03-18)

- Added `SeniorLatitude`/`SeniorLongitude` to `OrderDto`
- AutoMapper `ForMember` mappings from `Order.Senior.Contact.Latitude/Longitude`
- Enables Haversine distance calculation in admin app
- **Commit:** `80d418e`

### Task 11: StudentQueryBuilder Lat/Lng Fix ✅ (2026-03-19)

- `StudentQueryBuilder.ExecuteWithDetailsAsync()` was missing `Latitude`/`Longitude` in `ContactInfoDto` projection
- Fixed: added `Latitude = s.Contact.Latitude, Longitude = s.Contact.Longitude`
- Resolved 5331 km distance bug (was showing ~5331 km because lat/lng were 0)

### Task 12: AdminDirectAssign — Instant JobInstance Generation ✅ (2026-03-20)

- `AdminDirectAssignAsync()` now generates `JobInstance` records immediately on assignment
- Added 3 new dependencies: `IHangfireRecurringJobService`, `IPricingConfigurationRepository`, `IJobInstanceRepository`
- New private method `GenerateJobInstancesForAssignmentAsync()` — loads pricing config, builds navigation props, calls `GenerateInstancesForAssignment`, saves via `AddRangeAsync`
- `OrderScheduleRepository.GetByIdAsync()` — added `.ThenInclude(o => o.Senior)` (needed for `order.Senior.CustomerId`)
- **Tested:** Order 9 → assigned student → 21 sessions generated immediately, status Pending → FullAssigned

### Task 13: StudentDashboard DurationHours Bug Fix ✅ (2026-03-18)

- `DashboardService.GetStudentDashboard()` was referencing `DurationHours` which doesn't exist on `JobInstance`
- Fixed to calculate from `EndTime - StartTime`

---

## Faza 5 — Suspension & Holidays ✅ (2026-03-22)

### Task 14: Suspension Check Middleware ✅

- `SuspensionCheckMiddleware.cs` — vraća 403 JSON za suspendirane korisnike
- Preskače: `/api/auth/*`, `/api/suspensions/*`, Admin role
- Registriran u `Program.cs` između Authentication i Authorization
- **Commit:** `a652bff`

### Task 15: Suspension Check in CreateOrder ✅

- `OrdersService.CreateOrderAsync()` — provjera na vrhu: `Senior→Customer→User→IsSuspended`
- Throw-a `ForbiddenException` ako je korisnik suspendiran
- **Commit:** `a652bff`

### Task 16: Croatian Holidays (Blagdani) ✅

- `CroatianHolidays.cs` — 13 fiksnih praznika + Computus algoritam za Uskrsni ponedjeljak i Tijelovo
- `HangfireRecurringJobService` koristi `isOvertimeDay = Sunday || CroatianHolidays.IsPublicHoliday(date)`
- Label promijenjen: "Nedjeljna satnica" → "Povećana satnica"
- **Commit:** `a652bff`

---

## Migrations Applied

1. `20260313130245_AddSundayHourlyRate` — SundayHourlyRate column + pricing history columns
2. `20260313133641_AddBidirectionalReviews` — ReviewType, Senior rating fields, one-to-many relationship
3. `20260313140442_AddPromoCodeSystem` — PromoCodes + PromoCodeUsages tables, Order.PromoCodeId FK

---

## Next Steps

- All 9 backend gap analysis items COMPLETE
- Suspension middleware + Croatian holidays COMPLETE
- Ready for frontend-backend integration
- **Za Sidney-a:** Preostali TODO-ovi su u `helpi_admin/docs/ROADMAP.md`

### Preostalo (iz ROADMAP.md):

1. **Backend integracija** — Admin app Mock → REST API (GLAVNI zadatak)
2. **Chat/Poruke sustav** — NIŠTA ne postoji u backendu! Nema ChatRoom/ChatMessage entiteta, nema ChatController, nema ChatHub. Frontend (admin + app) ima mock UI. Detalji u `helpi_admin/docs/ROADMAP.md`
3. **Integracije** — Stripe, Minimax, Mailgun, MailerLite, Firebase (produkcijski credentials potrebni)
4. **Suspension notifikacije** — Push + email kad se korisnik suspendira (ovisi o Firebase)
5. **Push notifikacije** — Firebase FCM za sve uloge
6. **Per-user preferencije** — SharedPreferences s userId u admin appu

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
