# Helpi Backend v2 — Progress

> Zadnja izmjena: 2026-04-12

## 📖 Za Sidney-a — Što čitati (sva 3 repoa)

| GitHub repo (splivalo/) | Fajl                             | Sadržaj                                          |
| ----------------------- | -------------------------------- | ------------------------------------------------ |
| **helpi_administrator** | **docs/ROADMAP.md**              | **Svi preostali TODO-ovi (START HERE)**          |
| helpi_administrator     | docs/PROGRESS.md                 | Admin app status (98% frontend done)             |
| helpi_administrator     | docs/ARCHITECTURE.md             | Admin tech stack, folder structure, UI standardi |
| helpi_administrator     | docs/PROJECT_HISTORY.md          | Kronologija odluka (Feb→Mart 2026)               |
| **helpi_backend_v2**    | **docs/PROGRESS.md (ovaj fajl)** | Backend task tracking (29 taskova ✅)            |
| helpi_backend_v2        | README.md                        | DB schema, use case flows, 19 LINQ queries       |
| helpi_backend_v2        | seeds/README.md                  | Test data, login credentials, promo codes        |
| helpi_apps              | README.md                        | App tech stack, Riverpod/SignalR info            |
| helpi_apps              | docs/ARCHITECTURE.md             | Folder structure, 64 fajlova, providers          |

---

## Overall Status: 100% backend gaps resolved + suspension + holidays + admin notifications + contract renewal auto-trigger + reschedule notifications + admin dashboard legacy cleanup + invoice retry system + dynamic pricing (student rates + intermediary margin) + travel buffer reconciliation + historical student payout snapshots + zero-warning backend cleanup + notification content overhaul + Google Drive archive (single master CSV) + **Chat system (real-time + REST)**

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

- Admin: `GET /api/dashboard/admin` — cleaned to 4 v2 KPI tiles only:
  - `adminProcessingOrders`
  - `adminActiveOrders`
  - `adminTotalStudents`
  - `adminTotalSeniors`
- Dashboard tile `type` i `changeType` sada izlaze kao string enum nazivi, tako da frontend više nije vezan uz krhke numeričke enum vrijednosti
- Student: `GET /api/dashboard/student/{studentId}` — 6 tiles:
  - `upcomingSessions`, `completedSessionsStudent`, `totalEarnings`, `myRating`, `contractDaysRemaining`, `workedHoursStudent`
- Senior: `GET /api/dashboard/senior/{seniorId}` — 4 tiles:
  - `activeOrders`, `completedSessionsSenior`, `totalSpent`, `myRatingSenior`
- Old `GET /api/dashboard` removed, replaced with role-specific endpoints
- Old placeholder/admin-v1 tile builders removed from `DashboardService`
- Legacy admin `DashboardTileType` enum članovi uklonjeni nakon stabilizacije string contracta
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

## Faza 6 — Admin Notifications ✅ (2026-03-23)

### Task 17: Admin Notification Creation for 7 Business Events ✅

- **Problem:** Backend had full notification infrastructure (HNotification entity, 31 NotificationTypes, HNotificationsController, SignalR NotificationHub, INotificationService) but NO business logic ever called `CreateAsync()` — notification table was always EMPTY
- **New infrastructure:**
  - `IUserRepository.GetAdminIdsAsync()` — efficient SQL query `WHERE UserType == Admin SELECT Id`
  - `INotificationService.StoreAndNotifyAdminsAsync(adminIds, builder)` — loops admin IDs, creates per-admin notification via factory, stores to DB + sends via SignalR
- **7 notification types wired:**
  1. `newStudentAdded` — AuthService (student registration)
  2. `newSeniorAdded` — AuthService (senior registration)
  3. `orderCancelled` — OrdersService (order cancel)
  4. `jobCancelled` — JobInstanceService (session cancel)
  5. `contractExpired` — StudentStatusService (contract expiry)
  6. `paymentSuccess` — PaymentService (successful charge)
  7. `paymentFailed` — PaymentService (failed charge)
- **Hardcoded adminId=1 replaced** — All 6 occurrences of `adminId = 1` replaced with `GetAdminIdsAsync()` for multi-admin support
- **Files modified:** 9 (2 interfaces, 5 services, 1 repository, 1 notification service)
- **Build:** 0 errors, 74 warnings (identical to baseline)
- **Commit:** `69aec15`

### Task 18: Reschedule & Reassignment Notifications ✅ (2026-04-01)

- Added `NotificationFactory.JobRescheduledNotification()` with HR/EN localization keys
- `JobInstanceService.HandleSimpleReschedule()` now stores+sends `JobRescheduled` to senior, assigned student, and admins
- `JobInstanceService.HandleReschedule()` now stores+sends `JobRescheduled` to senior and admins after full reschedule flow starts
- `NotificationFactory.ReassignmentStartNotification()` now localizes both `ReassignmentStarted` and `ReassignmentCompleted`
- `ReassignmentService` now emits admin `ReassignmentStarted` when manual admin action is needed and `ReassignmentCompleted` when replacement is finalized
- Verification: `Helpi.Application.csproj` build passed; full solution build was blocked only by local `Helpi.WebApi` DLL file lock

---

## Migrations Applied

1. `20260313130245_AddSundayHourlyRate` — SundayHourlyRate column + pricing history columns
2. `20260313133641_AddBidirectionalReviews` — ReviewType, Senior rating fields, one-to-many relationship
3. `20260313140442_AddPromoCodeSystem` — PromoCodes + PromoCodeUsages tables, Order.PromoCodeId FK
4. `20260402085244_AddInvoiceTrackingFields` — InvoiceCreationStatus, MinimaxInvoiceId, InvoiceRetryCount on PaymentTransactions

---

## Faza 7 — Invoice Retry System ✅ (2026-04-02)

### Task 19: Invoice Creation Tracking ✅

- **Problem:** Kad Stripe uspješno naplati, a Minimax API padne (server down), invoice se tiho izgubi — `HandlePaymentSuccess` progutava exception bez zapisa
- **Novo enum:** `InvoiceCreationStatus { NotAttempted, Created, Failed }` u `enums.cs`
- **Nova polja na `PaymentTransaction`:**
  - `InvoiceCreationStatus` — prati je li Minimax invoice kreiran
  - `MinimaxInvoiceId` — sprema Minimax invoice ID (zaštita od duplikata)
  - `InvoiceRetryCount` — broji pokušaje (max 3)
- **Fix `HandlePaymentSuccess`:** Sad zapisuje `Failed` status ako Minimax padne, umjesto da tiho proguta error
- **Migration:** `20260402085244_AddInvoiceTrackingFields`

### Task 20: Invoice Auto-Retry ✅

- **`RetryInvoiceAsync(transactionId)`** — siguran retry: provjeri `Status == Paid` (ne ponavlja Stripe), provjeri `InvoiceCreationStatus == Created` (ne kreira dupli invoice)
- **`RetryFailedInvoicesAsync()`** — Hangfire poziva, iterira sve `Paid + Failed + RetryCount < 3`
- **Hangfire job:** `retry-failed-invoices` — trči svaki sat na :15
- **Registered u `Program.cs`** i `IJobInstanceJobs` interface

### Task 21: Admin Invoice Management Endpoint ✅

- **Novi controller:** `AdminPaymentController` (`api/admin/payments`)
- `POST /api/admin/payments/{id}/retry-invoice` — ručni retry za specifičnu transakciju
- `GET /api/admin/payments/failed-invoices` — lista svih failed invoicea
- Oba endpointa `[Authorize(Roles = "Admin")]`
- **Files:** 10 modified/created across Domain, Application, Infrastructure, WebApi

---

## Faza 8 — Dynamic Pricing (Student Rates) ✅ (2026-04-04)

### Task 22: StudentHourlyRate + StudentSundayHourlyRate ✅

- **PricingConfiguration entity** — Dodani `StudentHourlyRate` (default 7.40m), `StudentSundayHourlyRate` (default 11.10m)
- **DTO** — Oba polja dodana u `PricingConfigurationDto`
- **Validator** — `RuleFor(x => x.StudentHourlyRate).GreaterThan(0)` + isto za Sunday
- **Service** — Mapiranje u sva 4 metoda (GetAll, GetById, Add, Update) u `PricingConfigurationService`
- **AppDbContext** — `decimal(18,2)` column types za oba polja
- **Seeder** — Default 7.40m / 11.10m
- **Migracije:** `AddStudentRatesToPricingConfig`, `AddStudentRatesToPricingConfiguration`
- **Build:** 0 errors, ~73 warnings (baseline)

### Task 23: IntermediaryPercentage (Marža posrednika) ✅

- **PricingConfiguration entity** — Dodano `IntermediaryPercentage` (default 18m, raspon 0-100)
- **DTO + Validator** — `.GreaterThanOrEqualTo(0).LessThanOrEqualTo(100)`
- **Removed obsolete validation** — CompanyPercentage + ServiceProviderPercentage = 100 rule removed (legacy fields)
- **Migration:** `AddIntermediaryPercentageToPricingConfig`

### Task 24: Dynamic Travel Buffer Reconciliation ✅

- **AdminDirectAssignAsync** — više ne koristi hardkodirani `15`, nego čita `TravelBufferMinutes` iz `PricingConfiguration`
- **New service:** `TravelBufferReconciliationService`
- **Trigger point:** `PricingConfigurationService.UpdateConfigurationAsync()` nakon spremanja configa i history zapisa
- **Behaviour:** Ako je novi buffer veći od starog, servis grupira buduće `Upcoming` sesije po studentu i danu te za kasniji konfliktni accepted assignment pokreće postojeći `ReassignAssignment(..., CompleteTakeover, ...)`
- **Safety:** Reconciliation logira pojedinačne failove po assignmentu i ne ruši cijeli settings update
- **Live verification:** potvrđen realni lokalni DB scenarij za studenta Luku Perića na 2026-04-10: slot 11:15-12:15 prolazi s bufferom 15, pada s bufferom 20

### Task 25: Historical Student Payout Snapshot ✅

- **New field:** `JobInstance.StudentHourlyRate`
- **DTO contract:** `SessionDto` i `CompletedSessionDto` sada vraćaju `StudentHourlyRate`
- **Generation path:** `HangfireRecurringJobService` snapshota student weekday/sunday rate pri stvaranju novih `JobInstance` zapisa
- **Reschedule path:** `JobInstanceService` prenosi `StudentHourlyRate` pri cloneanju rescheduled sesije
- **Reason:** promjena studentske satnice u settingsu više ne smije prepisivati stare isplate, analytics ili student summary obračune
- **Migration:** `20260404103424_AddStudentHourlyRateSnapshotToJobInstances`
- **Validation:** snapshot field migriran i runtime potvrđen kroz `GET /api/sessions`

### Task 26: Backend Warning Cleanup to 0 ✅

- **Repository contracts aligned:** `GetById` / `GetByEmail` / `GetByContactId` / `GetDefaultPaymentMethod` potpisi usklađeni su s realnim nullable ponašanjem umjesto da backend lažno obećava non-null rezultat
- **Application services hardened:** `UserService`, `ContactInfoService`, `ReviewService`, `StudentsService`, `OrdersService` sada fail-fast prijavljuju missing entitete umjesto da se oslanjaju na implicitne null dereference
- **Infrastructure cleanup:** uklonjeni preostali EF false-positive include warningi i Minimax required-string assignment warning
- **Package/security debt:** AutoMapper upgrade i backend cleanup završeni bez regresije builda
- **Validation:** `dotnet build src\helpi_backend.sln` sada prolazi s `0` warninga; `flutter analyze` za admin i dalje vraća `No issues found!`

---

## Faza 9 — Notification Content Overhaul & Archive ✅ (2026-04-05)

### Task 27: FormatSafe Localization Fix ✅

- **Problem:** `JsonLocalizationService.GetString` crashao jer `String.Format` dobio `{0}` placeholder bez argumenata → 500 error na notification endpoint
- **Fix:** Dodan `FormatSafe(template, args)` helper — vraća template unchanged kad args prazni, wrappa `String.Format` u try/catch
- **File:** `JsonLocalizationService.cs`

### Task 28: TranslateNotifications Refactor ✅

- **Problem:** Monolitni if-else u `TranslateNotifications` s generičkim body formatom za sve tipove notifikacija
- **Refaktoriran** u specijalizirane grane:
  - `seniorAndOrderList` (JobCancelled, OrderCancelled, OrderScheduleCancelled, NewOrderAdded) → body `"{seniorName}, Narudžba #{orderId}"`
  - `reassignmentList` (ReassignmentStarted, ReassignmentCompleted) → isti format
  - `descList` (NoEligibleStudents, AllEligibleStudentNotified) → GetEntityDescription za body
  - `userDeletedList` → parse Payload JSON za deletedUserName/deletedUserId
  - `NewStudentAdded` / `NewSeniorAdded` → pravo ime iz dto.Student/Senior.Contact.FullName
- **NewOrderAdded dodano** — Novi lokalizacijski ključ u hr.json ("Nova narudžba") i en.json ("New Order")
- **NotificationsFactory fix** — `JobCancelledNotification` sad uključuje `OrderId = jobInstance.OrderId`
- **Files:** HNotificationService.cs, NotificationsFactory.cs, hr.json, en.json

### Task 29: Single Master CSV Archive ✅

- **Problem:** Svaki archive poziv kreirao novi fajl na Google Drive → proliferacija fajlova
- **Refaktoriran** `HNotificationsController.Archive`:
  - `FindFileInFolderAsync(folderId, "notifications-archive.csv")` → traži postojeći fajl
  - Ako postoji: `DownloadFileAsync` → strip BOM → append novi redovi → `UpdateFileAsync` (isti file ID)
  - Ako ne postoji: create novi fajl s headerom
  - CSV format: `Datum,Naslov,Poruka` (uklonjen Type stupac)
- **3 nove metode na IGoogleDriveService / GoogleDriveService:**
  - `FindFileInFolderAsync(folderId, fileName)` → vraća fileId ili null
  - `DownloadFileAsync(fileId)` → vraća byte[]
  - `UpdateFileAsync(fileId, data, mimeType)` → vraća webViewLink
- **DependencyInjection.cs** — Dodano mapiranje `NotificationsArchiveFolderId` iz konfiguracije (bilo propušteno)
- **HNotificationRepository** — Dodano `GetReadNotificationsByUserIdAsync(userId)` za dohvat pročitanih
- **HNotificationDto** — Dodan `SeniorName` property za CSV export
- **Files:** 13 files across Application + Infrastructure + WebApi
- **Testirano:** Prvi poziv kreira fajl, drugi poziv appendira na isti file ID
- **Build:** 0 errors, 0 new warnings

---

## Migrations Applied

1. `20260313130245_AddSundayHourlyRate`
2. `20260313133641_AddBidirectionalReviews`
3. `20260313140442_AddPromoCodeSystem`
4. `20260402085244_AddInvoiceTrackingFields`
5. `20260404085343_AddIntermediaryPercentageToPricingConfig`
6. `20260404093048_AddStudentRatesToPricingConfig`
7. `20260404093111_AddStudentRatesToPricingConfiguration`
8. `20260404103424_AddStudentHourlyRateSnapshotToJobInstances`
9. `20260412_AddChatSystem` — ChatRooms + ChatMessages tables

---

## Faza 10 — Chat System ✅ (2026-04-12)

### Task 30: Chat Entities + Migration ✅

- **ChatRoom entity** — Id, Participant1Id, Participant1Name, Participant2Id, Participant2Name, LastMessageContent, LastMessageAt, UnreadCount1, UnreadCount2, CreatedAt
- **ChatMessage entity** — Id, RoomId, SenderId, SenderName, Content, SentAt, IsRead
- **Migration** applied to PostgreSQL
- **DbContext** — ChatRooms, ChatMessages DbSets configured

### Task 31: ChatService + ChatRepository ✅

- **ChatRepository** — CRUD, GetUserRooms, GetRoomMessages (paged), IncrementUnread, ResetUnread
- **ChatService** — GetUserRoomsAsync (auto-creates admin room), GetOrCreateRoomAsync (welcome message), SendMessageAsync, MarkRoomAsReadAsync, GetUnreadCountAsync
- **GetUserDisplayNameAsync** — Admin shows as "Helpi", other users resolved via `GetByIdWithContactAsync` (eager includes Student.Contact + Customer.Contact)
- **Auto-room creation** — When user fetches rooms and has no admin room, one is created with welcome message

### Task 32: ChatController + ChatHub ✅

- **ChatController** (`api/chat`) — GET /rooms, POST /rooms, GET /rooms/{id}/messages, POST /rooms/{id}/messages, PUT /rooms/{id}/read, GET /unread-count
- **ChatHub** (`/hubs/chat`) — SignalR hub, JoinRoom/LeaveRoom groups
- **NotificationHub broadcast** — ChatController.SendMessage also broadcasts `ReceiveChatMessage` via NotificationHub to `user_{otherUserId}` group (both apps connect to NotificationHub, not ChatHub)
- **GetByIdWithContactAsync** — Added to IUserRepository/UserRepository for proper name resolution

---

## Next Steps

- All 9 backend gap analysis items COMPLETE
- Suspension middleware + Croatian holidays COMPLETE
- Admin notifications (7 types) — COMPLETE, SignalR delivery works
- Contract renewal auto-trigger — COMPLETE (JobInstances generated on upload)
- Reschedule and reassignment notifications — COMPLETE
- Smooth transition protection — COMPLETE (ReassignmentService won't expire students with next contract)
- Invoice retry system — COMPLETE (auto Hangfire hourly + admin manual endpoint)
- Seed data realistic overhaul — COMPLETE (admin user, 4 completed + 5 active + 5 pending, 47 sessions)
- Ready for frontend-backend integration
- Travel buffer reconciliation — COMPLETE
- Historical student payout snapshots — COMPLETE
- Backend warning cleanup to 0 — COMPLETE
- **Notification content overhaul** — COMPLETE (FormatSafe, TranslateNotifications refactor, NewOrderAdded)
- **Google Drive archive (single master CSV)** — COMPLETE (find/download/append/update flow, 3 new GoogleDriveService methods)
- **Chat system** — COMPLETE (ChatRoom, ChatMessage, ChatService, ChatController, ChatHub, NotificationHub broadcast, auto-room, welcome message, GetByIdWithContactAsync)
- **Za Sidney-a:** Preostali TODO-ovi su u `helpi_admin/docs/ROADMAP.md`

### Preostalo (iz ROADMAP.md):

1. **Integracije** — Stripe, Minimax, Mailgun, MailerLite, Firebase (produkcijski credentials potrebni)
2. **Suspension notifikacije** — Push + email kad se korisnik suspendira (ovisi o Firebase)
3. **Push notifikacije** — Firebase FCM za sve uloge
4. **Per-user preferencije** — SharedPreferences s userId u admin appu
5. **Sponzor sustav** — SponsorConfiguration entity + admin UI + app badge (branding)

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

### PricingConfiguration Authorization Smoke Test ✅ (2026-04-04)

- **Problem found during live API test:** običan authenticated customer mogao je čitati `GET /api/PricingConfiguration`
- **Fix:** `PricingConfigurationController` promijenjen na `[Authorize(Roles = "Admin")]`
- **Runtime verification:** Swagger v2 dokument učitan (`159` pathova), customer token vraća `403`, disposable admin token vraća `200` i ispravan pricing payload
- **Regression check:** `GET /api/sessions` i dalje vraća `studentHourlyRate` u runtime JSON contractu

### Student Dashboard DurationHours Fix ✅ (2026-03-18)

- **Problem:** `GET /api/dashboard/student/{id}` returned 500 Internal Server Error
- **Root cause:** `JobInstanceRepository.GetTotalCompletedHoursForPeriodAsync()` used `.SumAsync(ji => ji.DurationHours)` in LINQ, but `DurationHours` is a computed C# property (`EndTime - StartTime`), not a DB column — EF Core cannot translate it to SQL
- **Fix:** Changed to `ToListAsync()` + in-memory `.Sum(ji => ji.DurationHours)` — filters still run server-side, only Sum computed in memory on already-filtered results
- **File:** `Helpi.Infrastructure/Repositories/JobInstanceRepository.cs` line 338
- **Build:** 0 errors, no new warnings

### GET /api/reviews Endpoint Fix ✅ (2026-03-23)

- **Problem:** Admin app `GET /api/reviews` returned 405 Method Not Allowed — ReviewsController had NO root `[HttpGet]` endpoint
- **Fix:** Added `GetAllAsync()` across 4 layers:
  - `IReviewRepository.cs` — `Task<IEnumerable<Review>> GetAllAsync()`
  - `ReviewRepository.cs` — filters `IsPending == false`, includes Student + Senior, orders by CreatedAt desc
  - `ReviewService.cs` — `GetAllAsync()` maps to `List<ReviewDto>`
  - `ReviewsController.cs` — `[HttpGet] GetAll()` endpoint
- **Build:** 0 errors

### Backend Binding for Android Emulator ✅ (2026-03-23)

- **Problem:** `localhost:5142` only binds to 127.0.0.1 — Android emulator can't reach host via `10.0.2.2`
- **Fix:** Changed `launchSettings.json` applicationUrl from `http://localhost:5142` to `http://0.0.0.0:5142`

---

## Faza 7 — Contract Renewal & Service Continuity ✅ (2026-03-23)

### Task 18: Auto-generate JobInstances on Contract Upload ✅

- **Problem:** When student uploads a new contract (e.g. month-to-month renewal), NO new JobInstances were generated — only admin manual assign or Hangfire recurring batch triggered generation
- **Fix:** `StudentContractService.CreateContractAsync()` now calls `GenerateJobInstancesForStudentAssignmentsAsync()` after `ProcessStudentStatus()`
- **New method:** `GenerateJobInstancesForStudentAssignmentsAsync(studentId)` — fetches all active recurring assignments for the student, generates instances using `IHangfireRecurringJobService.GenerateInstancesForAssignment()`, saves via `AddRangeAsync`
- **3 new dependencies** added to StudentContractService: `IScheduleAssignmentRepository`, `IHangfireRecurringJobService`, `IPricingConfigurationRepository`
- **New repository method:** `IScheduleAssignmentRepository.GetAssignmentsNeedingJobGenerationForStudentAsync(studentId)` — same logic as `GetAssignmentsNeedingJobGenerationAsync()` but filtered by student, includes OrderSchedule→Order→Senior + latest JobInstance
- **Files modified:** 4 (StudentContractService.cs, IScheduleAssignmentRepository.cs, ScheduleAssignmentRepository.cs)
- **Build:** 0 errors

### Task 19: Smooth Transition Protection (Contract Renewal) ✅

- **Problem:** When student's contract expires but a new one starts immediately (gap ≤ 1 day), `StudentStatusService.HandleTrulyExpired()` would still mark student as Expired and trigger `ReassignExpiredContractJobs()` — reassigning all sessions even though student has valid next contract
- **Fix:** Added early return check in `HandleTrulyExpired()`: if `eval.NextContract != null && !eval.HasGap`, student stays Active and no reassignment occurs
- **Leverages existing:** `ContractEvaluationService.Evaluate()` already computes `HasGap` (gap > 1 day between contracts)
- **File modified:** StudentStatusService.cs
- **Build:** 0 errors

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

---

## Faza 7 Task Summary

| #   | Task                                            | Status | Date       |
| --- | ----------------------------------------------- | ------ | ---------- |
| 18  | Auto-generate JobInstances on contract upload   | ✅     | 2026-03-23 |
| 19  | Smooth transition protection (contract renewal) | ✅     | 2026-03-23 |
