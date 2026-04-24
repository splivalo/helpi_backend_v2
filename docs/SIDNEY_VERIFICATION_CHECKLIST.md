# Sidney Verification Checklist — 2026-04-24

> All backend tasks are complete. This is the verification/integration checklist for production.

---

## 📋 For Sidney — 7 External Integrations + Testing

### 1. ✅ Backend All Green

- [ ] `dotnet build` in `src/` folder — **0 errors** (only 80 legacy warnings expected)
- [ ] All 11+1 phases implemented (verify `PROGRESS.md`)
- [ ] `flutter analyze` in admin/app — **0 issues**

---

## 🔑 External Integrations (Require production credentials)

### 2. **Stripe** — Payment Processing

**What you need:**

- [ ] Stripe production API Key (Secret key)
- [ ] Stripe Webhook Secret (for fee tracking)

**What to do:**

1. Open `credentials/stripe.json` (prod folder or env var)
2. Update with production API key
3. Backend already has `PaymentService.InitiatePaymentAsync()` + `StripeWebhookHandler` implemented
4. **Testing:**
   - [ ] Create test senior + test student
   - [ ] Create order with payment
   - [ ] Verify fee is calculated correctly (17% for Helpi, rest to senior)
   - [ ] Check Stripe dashboard to confirm transaction is visible

**Files to review:**

- `Helpi.Infrastructure/Services/PaymentService.cs`
- `Helpi.WebApi/Controllers/PaymentsController.cs`
- `Helpi.Infrastructure/ExternalIntegrations/Stripe/StripeWebhookHandler.cs`

---

### 3. **Minimax** — SMS / Phone Validation

**What you need:**

- [ ] Minimax API credentials (API key / auth token)

**What to do:**

1. Open `credentials/minimax.json`
2. Update with production API key
3. Backend already has `MinimaxService.ValidatePhoneAsync()` implemented
4. **Testing:**
   - [ ] Register new senior with 🇭🇷 phone format (+385...)
   - [ ] Verify Minimax validated the phone number

**Files to review:**

- `Helpi.Infrastructure/ExternalIntegrations/Minimax/MinimaxService.cs`
- `Helpi.Application/Services/UserService.cs` (where validation is called)

---

### 4. **Mailgun** — Email Service

**What you need:**

- [ ] Mailgun API Key
- [ ] Mailgun Domain (e.g., `mg.helpi.hr` or similar)
- [ ] DNS MX records pointing to Mailgun

**What to do:**

1. Open `credentials/mailgun.json`
2. Update with production API key + domain
3. Backend already has `MailgunService.SendEmailAsync()` implemented
4. **Testing:**
   - [ ] Password reset → verify email arrived
   - [ ] Invoice generation → verify email with PDF arrived
   - [ ] Admin notification → verify email arrived

**Files to review:**

- `Helpi.Infrastructure/ExternalIntegrations/Mailgun/MailgunService.cs`

---

### 5. **MailerLite** — Newsletter / Marketing

**What you need:**

- [ ] MailerLite API Token
- [ ] Audience Group IDs (for different email lists)

**What to do:**

1. Open `credentials/mailerlite.json`
2. Update with production API token
3. Backend has `MailerLiteService` for adding contacts to groups
4. **Testing:**
   - [ ] Register new senior → verify added to MailerLite audience
   - [ ] Register new student → verify added to correct group

**Files to review:**

- `Helpi.Infrastructure/ExternalIntegrations/MailerLite/MailerLiteService.cs`

---

### 6. **Firebase** — Push Notifications + Suspend Notifications

**What you need:**

- [ ] Firebase service account JSON (for FCM)
- [ ] Firebase project ID

**What to do:**

1. Open `credentials/helpi-firebase-service-account.json`
2. Update with production Firebase service account
3. Backend has `FirebaseService.SendPushNotificationAsync()` implemented
4. App has FCM token registration implemented
5. **Testing:**
   - [ ] Senior receives push notification on new order
   - [ ] Student receives push notification when assigned
   - [ ] If user is suspended → push + email notification

**Files to review:**

- `Helpi.Infrastructure/ExternalIntegrations/Firebase/FirebaseService.cs`
- `Helpi.Application/Services/NotificationService.cs` (SuspendUserAsync)
- Admin app: `lib/features/settings/screens/settings_screen.dart` (notification testing)

---

### 7. **Stripe Webhook** — Fee Tracking (Depends on Stripe)

**What you need:**

- [ ] Stripe production webhook endpoint URL
- [ ] Database migration for fee tracking table

**What to do:**

1. Verify migration `AddStripeWebhookFeeTracking` is applied to prod DB
2. Set webhook in Stripe dashboard → `https://api.helpi.hr/webhooks/stripe`
3. Backend Hangfire job `ProcessStripeWebhookFeeAsync` will periodically check Stripe fees
4. **Testing:**
   - [ ] Create test transaction
   - [ ] Verify fee is correctly saved in DB `StripeWebhookEvents` table
   - [ ] Verify Hangfire job executed

**Files to review:**

- `Helpi.Infrastructure/ExternalIntegrations/Stripe/StripeWebhookHandler.cs`
- `Helpi.Infrastructure/Hangfire/Jobs/StripeWebhookProcessingJob.cs`

---

## 📱 Push Notifications (Depends on Firebase #6)

### Suspend User Notifications

**What you need:**

- [ ] Firebase credentials (from #6 above)

**What to do:**

1. Verify `UserService.SuspendUserAsync()` — sends push + email
2. App already has `notification_provider.dart` listening for push
3. **Testing:**
   - [ ] Admin suspends senior
   - [ ] Verify senior receives push notification
   - [ ] Verify senior receives email notification
   - [ ] Senior sees suspension notice on login screen

**Files to review:**

- `Helpi.Application/Services/UserService.cs` (SuspendUserAsync)
- `Helpi.Application/Services/NotificationService.cs` (PushNotification)

---

## 🔒 Security Checklist

### Before Production:

- [ ] All credentials are in `credentials/dev/` folder (NEVER hardcoded)
- [ ] All env vars are available on prod deployment
- [ ] Rate limiting is active (Hangfire, API endpoints)
- [ ] HTTPS is mandatory for prod (JWT security)
- [ ] CORS is restricted to `https://helpi.example.com` only (not `*`)
- [ ] Sensitive debugPrint() removed (checked ✅ in Phase 11)
- [ ] DomainException-s do not leak raw exception details to user (checked ✅)
- [ ] IDOR vulnerability fixed (PUT /orders/{id} → admin only) ✅

---

## ✅ What Sidney Needs to Test — E2E Flow

### Scenario 1: Senior Registration + First Order

1. [ ] **Senior Registration**
   - Enter name, last name, email, phone, location
   - Minimax validates phone number (credentials OK)
   - Email sent
   - Senior profile created in DB

2. [ ] **Student Sees Senior + Orders**
   - Student searches seniors
   - Select senior
   - Create order with date/time
   - Select payment method (Stripe)

3. [ ] **Payment**
   - Click "Pay"
   - Redirect to Stripe payment form
   - Enter test card `4242 4242 4242 4242` (future date, CVC 123)
   - Confirm payment
   - Verify transaction in `JobInstances` + `Invoices` table
   - Verify senior's earning calculation is correct (17% Helpi fee)

4. [ ] **Notifications**
   - Senior receives push: "New order, Student Name"
   - Senior receives email with details
   - Student receives confirmation email

5. [ ] **Session + Invoice**
   - Admin can see session in order (scheduled time)
   - Verify invoice generated and emailed to senior/student

---

### Scenario 2: Suspend User (Firebase Test)

1. [ ] Admin suspends senior
2. [ ] Senior receives push notification
3. [ ] Senior receives email with reason
4. [ ] Senior attempts login → sees suspension notice
5. [ ] Senior cannot see orders/sessions anymore

---

### Scenario 3: Google Drive Archive (Notifications)

1. [ ] Admin goes to Notifications screen
2. [ ] Click "Archive"
3. [ ] Verify new file created or appended to Google Drive: `notifications-archive.csv`
4. [ ] Verify CSV format: `Date,Title,Message`

---

## 📞 If You Need Help

If `flutter analyze` or `dotnet build` throws errors, report:

- [ ] Exact error message (copy-paste from terminal)
- [ ] Which file?
- [ ] Which line?

If integration doesn't work, verify:

- [ ] Is credentials JSON valid? (Paste in JSON validator)
- [ ] Is API key correct? (Test in Postman)
- [ ] Is firewall/VPN blocking the request?

---

**Status for Sidney:** Backend is 100% complete. Now we need only credentials + E2E testing. ✅
