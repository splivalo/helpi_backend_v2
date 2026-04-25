# Helpi Backend v2 — Progress

> Last updated: 2026-04-25

## 2026-04-25 - Student DateOfBirth Fix

- ✅ **BUG FIX**: `StudentQueryBuilder.ExecuteWithDetailsAsync()` was missing `DateOfBirth` in the manual `ContactInfoDto` projection
- **Root cause**: Seniors use AutoMapper (maps all properties automatically), students use hand-written LINQ projection which forgot `DateOfBirth`
- **Result**: Student date of birth now properly returned via `GET /api/students` endpoint

## 📖 For Sidney — What to Read (all 3 repos)

| GitHub repo (splivalo/) | File                             | Content                                          |
| ----------------------- | -------------------------------- | ------------------------------------------------ |
| **helpi_administrator** | **docs/ROADMAP.md**              | **All remaining TODOs (START HERE)**             |
| helpi_administrator     | docs/PROGRESS.md                 | Admin app status (98% frontend done)             |
| helpi_administrator     | docs/ARCHITECTURE.md             | Admin tech stack, folder structure, UI standards |
| helpi_administrator     | docs/PROJECT_HISTORY.md          | Chronology of decisions (Feb→March 2026)         |
| **helpi_backend_v2**    | **docs/PROGRESS.md (this file)** | Backend task tracking (29 tasks ✅)              |
| helpi_backend_v2        | README.md                        | DB schema, use case flows, 19 LINQ queries       |
| helpi_backend_v2        | seeds/README.md                  | Test data, login credentials, promo codes        |
| helpi_apps              | README.md                        | App tech stack, Riverpod/SignalR info            |
| helpi_apps              | docs/ARCHITECTURE.md             | Folder structure, 64 files, providers            |

---

## Overall Status: 100% backend gaps resolved + suspension + holidays + admin notifications + contract renewal auto-trigger + reschedule notifications + admin dashboard legacy cleanup + invoice retry system + dynamic pricing (student rates + intermediary margin) + travel buffer reconciliation + historical student payout snapshots + zero-warning backend cleanup + notification content overhaul + Google Drive archive (single master CSV) + **Chat system (real-time + REST)** + **PromoCode→Coupon unification** + **CouponType simplification (3 hour-based types only)** + **Student assignment acceptance (PendingAcceptance flow)** + **Cancel/Availability admin toggles**

---

## Phase 1 — Security & Core Fixes ✅

### Task 1: Senior Edit Prevention ✅

- `PUT /orders/{id}` restricted to Admin role only
- Senior cannot edit orders (only cancel)

### Task 2: Order Cancel 24h Rule ✅

- Cancel endpoint enforces 24h-before-service time check
- Throws DomainException if less than 24h before service start

---

## Phase 2 — Core Business Logic ✅

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

## Phase 3 — Extended Features ✅

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

### Task 8: Dashboard Per-Role ✅

- Admin: `GET /api/dashboard/admin` — cleaned to 4 v2 KPI tiles only:
  - `adminProcessingOrders`, `adminActiveOrders`, `adminTotalStudents`, `adminTotalSeniors`
- Dashboard tile `type` and `changeType` now output as string enum names
- Student: `GET /api/dashboard/student/{studentId}` — 6 tiles:
  - `upcomingSessions`, `completedSessionsStudent`, `totalEarnings`, `myRating`, `contractDaysRemaining`, `workedHoursStudent`
- Senior: `GET /api/dashboard/senior/{seniorId}` — 4 tiles:
  - `activeOrders`, `completedSessionsSenior`, `totalSpent`, `myRatingSenior`
- Legacy admin `DashboardTileType` enum members removed after stabilizing string contract
- Current month vs last month comparison with percentage change

### Task 9: Session Terminology ✅

- Route: `api/job-instances` → `api/sessions`
- Controller: `JobInstancesController` → `SessionsController`
- DTO renames: `JobInstanceDto` → `SessionDto`, `CompletedJobInstanceDto` → `CompletedSessionDto`, `JobInstanceUpdateDto` → `SessionUpdateDto`, `JobInstanceIncludeOptions` → `SessionIncludeOptions`, `ManageJobInstanceRequestDto` → `ManageSessionRequestDto`
- Property rename: `StudentContractDto.JobInstances` → `Sessions` (with explicit AutoMapper mapping)
- Domain entity remains `JobInstance` (internal), API speaks "Session"

### Task 6: Promo Code System ✅

- **Entities:** `PromoCode` + `PromoCodeUsage`
- **Enum:** `PromoCodeType { Percentage, FixedAmount }`
- **Order integration:** `Order.PromoCodeId` optional FK
- **Controller:** `api/promo-codes` — Admin CRUD + public validate/apply endpoints
- **Validation logic:** percentage ≤ 100%, one use per customer per code, date range, max uses
- **Migration:** `20260313140442_AddPromoCodeSystem`

---

## Phase 4 — Admin Assign & Distance Improvements ✅

### Task 10: OrderDto Senior Lat/Lng ✅ (2026-03-18)

- Added `SeniorLatitude`/`SeniorLongitude` to `OrderDto`
- Enables Haversine distance calculation in admin app

### Task 11: StudentQueryBuilder Lat/Lng Fix ✅ (2026-03-19)

- `StudentQueryBuilder.ExecuteWithDetailsAsync()` was missing `Latitude`/`Longitude` in `ContactInfoDto` projection
- Fixed: added `Latitude = s.Contact.Latitude, Longitude = s.Contact.Longitude`
- Resolved 5331 km distance bug

### Task 12: AdminDirectAssign — Instant JobInstance Generation ✅ (2026-03-20)

- `AdminDirectAssignAsync()` now generates `JobInstance` records immediately on assignment
- New private method `GenerateJobInstancesForAssignmentAsync()` — loads pricing config, builds navigation props, calls `GenerateInstancesForAssignment`, saves via `AddRangeAsync`
- **Tested:** Order 9 → assigned student → 21 sessions generated immediately

### Task 13: StudentDashboard DurationHours Bug Fix ✅ (2026-03-18)

- `DashboardService.GetStudentDashboard()` was referencing `DurationHours` which doesn't exist on `JobInstance`
- Fixed to calculate from `EndTime - StartTime`

---

## Phase 5 — Suspension & Holidays ✅ (2026-03-22)

### Task 14: Suspension Check Middleware ✅

- `SuspensionCheckMiddleware.cs` — returns 403 JSON for suspended users
- Skips: `/api/auth/*`, `/api/suspensions/*`, Admin role

### Task 15: Suspension Check in CreateOrder ✅

- `OrdersService.CreateOrderAsync()` — check at top: `Senior→Customer→User→IsSuspended`
- Throws `ForbiddenException` if user suspended

### Task 16: Croatian Holidays ✅

- `CroatianHolidays.cs` — 13 fixed holidays + Computus algorithm for Easter Monday and Corpus Christi
- `HangfireRecurringJobService` uses `isOvertimeDay = Sunday || CroatianHolidays.IsPublicHoliday(date)`
- Label changed: "Sunday rate" → "Increased rate"

---

## Phase 6 — Admin Notifications ✅ (2026-03-23)

### Task 17: Admin Notification Creation for 7 Business Events ✅

- **7 notification types wired:**
  1. `newStudentAdded` — AuthService
  2. `newSeniorAdded` — AuthService
  3. `orderCancelled` — OrdersService
  4. `jobCancelled` — JobInstanceService
  5. `contractExpired` — StudentStatusService
  6. `paymentSuccess` — PaymentService
  7. `paymentFailed` — PaymentService
- **Multi-admin support** — `GetAdminIdsAsync()` replaces hardcoded `adminId = 1`

### Task 18: Reschedule & Reassignment Notifications ✅ (2026-04-01)

- Added `NotificationFactory.JobRescheduledNotification()`
- `JobInstanceService.HandleSimpleReschedule()` now stores+sends `JobRescheduled` to senior, student, and admins
- `ReassignmentService` now emits admin `ReassignmentStarted` and `ReassignmentCompleted`

---

## Phase 7 — Invoice Retry System ✅ (2026-04-02)

### Task 19: Invoice Creation Tracking ✅

- **New enum:** `InvoiceCreationStatus { NotAttempted, Created, Failed }`
- **New fields on `PaymentTransaction`:** `InvoiceCreationStatus`, `MinimaxInvoiceId`, `InvoiceRetryCount`
- **Fix `HandlePaymentSuccess`:** Now records `Failed` status if Minimax fails, instead of swallowing error

### Task 20: Invoice Auto-Retry ✅

- **`RetryFailedInvoicesAsync()`** — Hangfire job calls every hour at :15, iterates `Paid + Failed + RetryCount < 3`

### Task 21: Admin Invoice Management Endpoint ✅

- `POST /api/admin/payments/{id}/retry-invoice` — manual retry
- `GET /api/admin/payments/failed-invoices` — list all failed invoices

---

## Phase 8 — Dynamic Pricing (Student Rates) ✅ (2026-04-04)

### Task 22: StudentHourlyRate + StudentSundayHourlyRate ✅

- **PricingConfiguration entity** — Added `StudentHourlyRate` (default 7.40m), `StudentSundayHourlyRate` (default 11.10m)
- **Migrations:** `AddStudentRatesToPricingConfig`, `AddStudentRatesToPricingConfiguration`

### Task 23: IntermediaryPercentage (Student Service Fee) ✅

- **PricingConfiguration entity** — Added `IntermediaryPercentage` (default 18m, range 0-100)
- **Migration:** `AddIntermediaryPercentageToPricingConfig`

### Task 24: Dynamic Travel Buffer Reconciliation ✅

- **AdminDirectAssignAsync** — now reads `TravelBufferMinutes` from `PricingConfiguration`
- **New service:** `TravelBufferReconciliationService` — triggered on configuration update. If new buffer is larger, checks future sessions and starts reassignment for conflicts.

### Task 25: Historical Student Payout Snapshot ✅

- **New field:** `JobInstance.StudentHourlyRate` — snapshots rate at creation time.
- **Reason:** changing student rates in settings must not overwrite past payouts or analytics.
- **Migration:** `20260404103424_AddStudentHourlyRateSnapshotToJobInstances`

### Task 26: Backend Warning Cleanup to 0 ✅

- **Repository contracts aligned:** Fixed nullable behavior for `GetById`, `GetByEmail`, etc.
- **Application services hardened:** Added null checks and fail-fast DomainExceptions.
- **Validation:** `dotnet build src\helpi_backend.sln` now passes with `0` warnings.

---

## Phase 9 — Notification Content Overhaul & Archive ✅ (2026-04-05)

### Task 27: FormatSafe Localization Fix ✅

- **Fix:** Added `FormatSafe(template, args)` helper — returns template unchanged when args empty, wraps `String.Format` in try/catch.

### Task 28: TranslateNotifications Refactor ✅

- **Refactored** into specialized branches: `seniorAndOrderList`, `reassignmentList`, `descList`, `userDeletedList`, etc.
- **NewOrderAdded added** — New localization keys in hr.json and en.json.

### Task 29: Single Master CSV Archive ✅

- **Refactored** `HNotificationsController.Archive`: now appends to a single `notifications-archive.csv` on Google Drive instead of creating new files.
- **3 new methods on IGoogleDriveService:** `FindFileInFolderAsync`, `DownloadFileAsync`, `UpdateFileAsync`.

---

## Phase 10 — Chat System ✅ (2026-04-12)

### Task 30: Chat Entities + Migration ✅

- **ChatRoom** and **ChatMessage** entities created and migrated.

### Task 31: ChatService + ChatRepository ✅

- **Auto-room creation** — Admin room created automatically for new users with welcome message.
- **GetUserDisplayNameAsync** — Admin shows as "Helpi", others resolved via `GetByIdWithContactAsync`.

### Task 32: ChatController + ChatHub ✅

- **ChatHub** (`/hubs/chat`) for real-time messages.
- **NotificationHub broadcast** — Sends `ReceiveChatMessage` to both apps.

---

## Phase 11 — Security & Code Quality ✅ (2026-04-24)

### Task 33: IDOR Fix — ScheduleAssignmentsController ✅

- Added auth checks: Caller must be Admin or the Student owning the data.
- Fixed `int.Parse` crash potential in JWT claims.

### Task 34: ExceptionHandlingMiddleware — Info Leak Fix ✅

- Exception messages only returned to client in Development mode.
- Removed internal `source` field from error response.

### Task 35: ChatService KeyNotFoundException → DomainException ✅

- Standardized error handling to 400 DomainException.

### Task 36: ContactInfoService Generic Exception → DomainException ✅

- Standardized city resolution failures.

### Task 37: OrdersController — Useless Catch Removed ✅

- Removed empty try-catch wrappers.

### Task 38: PaymentTransactionService Dead Code Removed ✅

- Cleaned up commented-out methods.

---

## Next Steps

- All 9 backend gap analysis items COMPLETE
- Suspension middleware + Croatian holidays COMPLETE
- Admin notifications (7 types) — COMPLETE
- Contract renewal auto-trigger — COMPLETE
- Reschedule and reassignment notifications — COMPLETE
- Invoice retry system — COMPLETE
- Chat system (real-time + REST) — COMPLETE
- Security & Code Quality — COMPLETE
- **For Sidney:** Remaining TODOs are in `helpi_admin/docs/ROADMAP.md`

### Remaining (from ROADMAP.md):

1. **Integrations** — Stripe, Minimax, Mailgun, MailerLite, Firebase (production credentials needed)
2. **Suspension notifications** — Push + email when user suspended (depends on Firebase)
3. **Push notifications** — Firebase FCM for all roles

---

## 2026-04-20 — Student Settings, PendingAcceptance, canCancel stamping

### Student Settings permissions (PricingConfiguration)

- `PricingConfiguration` — 3 new fields: `StudentCancelEnabled`, `AvailabilityChangeEnabled`, `AvailabilityChangeCutoffHours`.
- `AssignmentStatus.PendingAcceptance` — new status: student must confirm assignment.
- `StampCanCancelAsync(sessions, callerRole)` — backend stamps `canCancel: true/false` on each session based on rules.

### Session date range filter

- `GET /api/sessions/order/{orderId}?from=&to=` — optional date range filter for order details.

### OrderStatusUpdater revision

- Automatically concludes order status (Completed/Cancelled) based on session states.

---

## Fixes & Improvements

### Delete Account Notification — Real Name ✅

- Notifications now show student/senior full name instead of ID only.

### Email Availability Check Endpoint ✅

- `GET /api/auth/check-email?email=xxx` — public endpoint for pre-registration validation.

### Swagger v2 ✅

- Changed Swagger doc from v1 to v2.
