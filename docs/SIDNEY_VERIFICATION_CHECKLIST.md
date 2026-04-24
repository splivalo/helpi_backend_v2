# Sidney Verification Checklist — 2026-04-24

> Svi backend tasking su gotovi. Ovo je lista što trebam PROVJERITI / INTEGRIRATI za produkciju.

---

## 📋 Za Sidney-a — 7 Vanjskih Integracija + Testiranje

### 1. ✅ Backend All Green
- [ ] `dotnet build` u `src/` folder — **0 errors** (samo 80 legacy warninga očekivani)
- [ ] Svi 11+1 faze implementirane (provjerite `PROGRESS.md`)
- [ ] `flutter analyze` u admin/app — **0 issues**

---

## 🔑 Vanjske Integracije (Trebaju produkcijski credentials)

### 2. **Stripe** — Payment Processing

**Što trebam:**
- [ ] Stripe production API Key (Secret key)
- [ ] Stripe Webhook Secret (za fee tracking)

**Što trebam napraviti:**
1. Otvori `credentials/stripe.json` (prod folder ili env var)
2. Ažuriraj s produkcijskim keyom
3. Backend már ima `PaymentService.InitiatePaymentAsync()` + `StripeWebhookHandler` implementiran
4. **Testiranje:** 
   - [ ] Kreiraj test senior + test student
   - [ ] Napravi order s plaćanjem
   - [ ] Provjeri da se provizija obračunala (17% za Helpi, ostatak senioru)
   - [ ] Provjeri u Stripe dashboard je li transakcija vidljiva

**Datoteke što trebam pregledati:**
- `Helpi.Infrastructure/Services/PaymentService.cs`
- `Helpi.WebApi/Controllers/PaymentsController.cs`
- `Helpi.Infrastructure/ExternalIntegrations/Stripe/StripeWebhookHandler.cs`

---

### 3. **Minimax** — SMS / Phone Validation

**Što trebam:**
- [ ] Minimax API credentials (API key / auth token)

**Što trebam napraviti:**
1. Otvori `credentials/minimax.json`
2. Ažuriraj s produkcijskim keyom
3. Backend već ima `MinimaxService.ValidatePhoneAsync()` implementiran
4. **Testiranje:**
   - [ ] Registracija novog seniora s brojem 🇭🇷 format (+385...)
   - [ ] Provjeri Minimax je li validirao broj

**Datoteke što trebam pregledati:**
- `Helpi.Infrastructure/ExternalIntegrations/Minimax/MinimaxService.cs`
- `Helpi.Application/Services/UserService.cs` (gdje se poziva validacija)

---

### 4. **Mailgun** — Email Service

**Što trebam:**
- [ ] Mailgun API Key
- [ ] Mailgun Domain (npr. `mg.helpi.hr` ili slično)
- [ ] DNS MX records pointing to Mailgun

**Što trebam napraviti:**
1. Otvori `credentials/mailgun.json`
2. Ažuriraj s produkcijskim keyom + domain
3. Backend već ima `MailgunService.SendEmailAsync()` implementiran
4. **Testiranje:**
   - [ ] Reset password → provjeri je li email stigao
   - [ ] Invoice generation → provjeri je li email sa PDF-om stigao
   - [ ] Admin notification → provjeri je li email stigao

**Datoteke što trebam pregledati:**
- `Helpi.Infrastructure/ExternalIntegrations/Mailgun/MailgunService.cs`

---

### 5. **MailerLite** — Newsletter / Marketing

**Što trebam:**
- [ ] MailerLite API Token
- [ ] Audience Group ID-ove (za različite email liste)

**Što trebam napraviti:**
1. Otvori `credentials/mailerlite.json`
2. Ažuriraj s produkcijskim keyom
3. Backend ima `MailerLiteService` za dodavanje kontakata u groupe
4. **Testiranje:**
   - [ ] Registracija novog seniora → provjeri je li dodan u MailerLite audience
   - [ ] Registracija novog studenta → provjeri je li dodan u pravi group

**Datoteke što trebam pregledati:**
- `Helpi.Infrastructure/ExternalIntegrations/MailerLite/MailerLiteService.cs`

---

### 6. **Firebase** — Push Notifications + Suspend Notifications

**Što trebam:**
- [ ] Firebase service account JSON (za FCM)
- [ ] Firebase project ID

**Što trebam napraviti:**
1. Otvori `credentials/helpi-firebase-service-account.json`
2. Ažuriraj s produkcijskim Firebase service accountom
3. Backend ima `FirebaseService.SendPushNotificationAsync()` implementiran
4. App ima FCM token registration implementiran
5. **Testiranje:**
   - [ ] Senior dobija push notifikaciju na novo narudžbu
   - [ ] Student dobija push notifikaciju kad je dodijeljen
   - [ ] Ako je korisnik suspendiran → push + email notifikacija

**Datoteke što trebam pregledati:**
- `Helpi.Infrastructure/ExternalIntegrations/Firebase/FirebaseService.cs`
- `Helpi.Application/Services/NotificationService.cs` (SuspendUserAsync)
- Admin app: `lib/features/settings/screens/settings_screen.dart` (notification testing)

---

### 7. **Stripe Webhook** — Fee Tracking (Ovisi o Stripe)

**Što trebam:**
- [ ] Stripe production webhook endpoint URL
- [ ] Database migration za fee tracking tablica

**Što trebam napraviti:**
1. Provjerite je li migracija `AddStripeWebhookFeeTracking` primijenjena na prod DB
2. Postavite webhook u Stripe dashboard → `https://api.helpi.hr/webhooks/stripe`
3. Backend Hangfire job `ProcessStripeWebhookFeeAsync` će periodički provjeravati Stripe fees
4. **Testiranje:**
   - [ ] Kreiraj test transakciju
   - [ ] Provjerite je li fee pravilno snimljen u DB `StripeWebhookEvents` tablica
   - [ ] Provjeri je li Hangfire job pokrenut

**Datoteke što trebam pregledati:**
- `Helpi.Infrastructure/ExternalIntegrations/Stripe/StripeWebhookHandler.cs`
- `Helpi.Infrastructure/Hangfire/Jobs/StripeWebhookProcessingJob.cs`

---

## 📱 Push Notifications (Ovisi o Firebase #6)

### Suspend User Notifications

**Što trebam:**
- [ ] Firebase credentials (iz #6 gore)

**Što trebam napraviti:**
1. Provjerite `UserService.SuspendUserAsync()` — pošalje push + email
2. App već ima `notification_provider.dart` koji slušaj push
3. **Testiranje:**
   - [ ] Admin suspendira seniora
   - [ ] Provjeri senior dobija push notifikaciju
   - [ ] Provjeri senior dobija email notifikaciju
   - [ ] Senior vidi suspension notice na login ekranu

**Datoteke što trebam pregledati:**
- `Helpi.Application/Services/UserService.cs` (SuspendUserAsync)
- `Helpi.Application/Services/NotificationService.cs` (PushNotification)

---

## 🔒 Security Checklist

### Before Production:

- [ ] Svi credentials su u `credentials/dev/` folder (NIKADA hardcoded)
- [ ] Svi env vars su dostupni na prod deployment
- [ ] Rate limiting je aktivan (Hangfire, API endpoints)
- [ ] HTTPS je obavezna za prod (JWT sigurnost)
- [ ] CORS je ograničen samo na `https://helpi.example.com` (ne `*`)
- [ ] Sensitive debugPrint() su uklonjeni (checked ✅ u Faza 11)
- [ ] DomainException-i ne curre raw exception details korisniku (checked ✅)
- [ ] IDOR vulnerability riješena (PUT /orders/{id} → admin only) ✅

---

## ✅ Što Trebam da Testira Sidney — E2E Tok

### Scenario 1: Senior Registration + First Order

1. [ ] **Registracija seniora**
   - Upiši ime, prezime, email, tel, lokacija
   - Minimaxvalidira telefon (credentials OK)
   - Email poslane
   - Kreira se Senior profil u DB

2. [ ] **Student vidi seniora + naručuje**
   - Student pretraži studente
   - Odaberi seniora
   - Kreiraj order s datumom/vremenom
   - Odaberi payment method (Stripe)

3. [ ] **Plaćanje**
   - Klikni "Plati"
   - Redirect na Stripe payment form
   - Unesi test karticu `4242 4242 4242 4242` (future date, CVC 123)
   - Potvrdi plaćanje
   - Provjerite je li transakcija u `JobInstances` + `Invoices` tablica
   - Provjerite je li obračun seniorova zarade točan (17% Helpi provizija)

4. [ ] **Notifikacije**
   - Senior dobija push: "Nova narudžba, Student Name"
   - Senior dobija email s detalja
   - Student dobija confirmation email

5. [ ] **Session + Invoice**
   - Admin može vidjeti sesiju u narudžbi (scheduled time)
   - Provjeri je li invoice generiran i emajliran senioru/studentu

---

### Scenario 2: Suspend User (Firebase Test)

1. [ ] Admin suspendira seniora
2. [ ] Senior dobija push notifikaciju
3. [ ] Senior dobija email s razlogom
4. [ ] Senior pokušaj login → vidiokaz suspension notice
5. [ ] Senior ne može više vidjeti narudžbe/sessions

---

### Scenario 3: Google Drive Archive (Notifications)

1. [ ] Admin ide na Notifikacije ekran
2. [ ] Klikne "Arhiviraj" 
3. [ ] Provjeri se nova datoteka kreira ili append na Google Drive: `notifications-archive.csv`
4. [ ] Provjeri CSV format: `Datum,Naslov,Poruka`

---

## 📞 Ako Trebam Pomoć

Ako `flutter analyze` ili `dotnet build` baca errore, javi:
- [ ] Točan error message (copy-paste iz terminala)
- [ ] Koji fajl?
- [ ] Koji redak?

Ako integracija ne radi, provjeri:
- [ ] Je li credentials JSON validan? (Try paste u JSON validator)
- [ ] Je li API key točan? (Test u Postman)
- [ ] Je li firewall/VPN blokirajući zahtjev?

---

**Status za Sidney:** Backend je 100% gotov. Sada trebam samo credentials + E2E testiranje. ✅

