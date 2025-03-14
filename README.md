```bash
Helpi.Backend/
│
├── Helpi.Domain/           # Core entities, value objects
│   ├── Entities/
│   ├── Interfaces/
│   └── ValueObjects/
│
├── Helpi.Application/      # Use cases, DTOs, service interfaces
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Services/
│   └── UseCases/
│
├── Helpi.Infrastructure/   # Database, external services
│   ├── Persistence/
│   ├── Repositories/
│   └── ExternalServices/
│
└── Helpi.WebApi/           # Controllers,  DI configuration
    ├── Controllers/
    ├── Configurations/
    └── Program.cs
```
# Helpi Database Schema Documentation

## 📌 Introduction
Helpi is an app that connects **seniors** who need services with **students** who provide them. The schema supports:
- **User authentication** (Admins, Students, Customers)
- **Service selection & scheduling** (Availability, Recurring Orders, One-time Orders)
- **Order Management** (Job Requests, Assignments, Replacements)
- **Payments & Invoicing**
- **Location-based Service Management**
- **Feedback & Review System**

---

## 🛠 **Key Schema Design Decisions**
### **1️⃣ User Authentication & Identity**
- Users are classified as **Admins, Students, or Customers** (`Users` table).
- **Students & Customers** have additional details stored in `Students` and `Customers` tables.
- `ContactInfo` is a separate table shared across different entities.

### **2️⃣ Order & Scheduling System**
- **Seniors** place an order (one-time or recurring) for a service (`Orders` table).
- Recurring orders specify the days and times (`OrderSchedules` table).
- **Job Requests** are sent to eligible students (`JobRequests` entries created). If a student **does not respond within 10 minutes**, the request is sent to another student.
- If no single student can fulfill all required days, the system assigns the **best-fit** student first, then fills the remaining slots with others.
- If a **student is sick** or cannot work a specific day, a **temporary replacement** is assigned.

---

## 🔄 **Use Case Flow: Senior Places a Service Order**
### **Flow:**
1. **Senior selects services**:
   - Senior selects one or more services from available categories.

2. **Senior picks time slots**:
   - Senior chooses specific days and times for the service bundle.

3. **System verifies**:
   - Services are available in the senior's area (`ServiceRegions`).
   - Students qualified for all services exist in the region.

4. **Order is created**:
   - Master record is created in `Orders`.
   - Service details are stored in `OrderServices` (service list + quantities).
   - Schedules are generated (`OrderSchedules` entries for chosen slots).

5. **Matching system starts**:
   - Finds students available for all ordered services.
   - Checks `StudentAvailabilitySlots` and qualifications.
   - Sends job requests (`JobRequests` table).
   - Auto-retries with new students every 10 minutes if unaccepted.

6. **Student accepts request**:
   - `ScheduleAssignments` links student to the entire order.

7. **Services executed**:
   - Single `JobInstances` entry per scheduled time slot.
   - Tracks completion of all services in that slot.

8. **Senior charged**:
   - `PaymentTransactions` aggregates costs from all services.
   - Charged 10 minutes before each service slot.

9. **Invoice generated**:
   - Consolidated `Invoices` for all services in the order.
   - Sent via `InvoiceEmails`.

---

#### **Affected Tables:**
| Table | Purpose |
|-------|---------|
| `Orders` | Stores the senior's service request, type, and status. |
|  `OrderServices`  | Lists services in order + price snapshots   |
| `OrderSchedules` |  Defines the specific **days and times** the services will occur. |
| `ServiceRegions` | Ensures that the requested service is available in the senior's area. |
| `JobRequests` | Tracks job offers sent to students. |
| `ScheduleAssignments` | Assigns students to services based on availability. |
| `JobInstances` | Tracks the execution of each scheduled job. |
| `PaymentTransactions` | Stores payment records and ensures payments are processed for completed services. |
| `Invoices` | Generates invoices once a payment is processed. |
| `InvoiceEmails` | Sends notifications about invoices. |

---

## 🔄 **Use Case Flow: Handling Recurring Services & Student Absence**
### **Flow:**
1. **Senior books a recurring service** (e.g., every Monday & Wednesday).
2. **System creates multiple schedules** (`OrderSchedules` table).
3. **Students are assigned for all sessions** (`ScheduleAssignments`).
4. If a **student cancels for a single day**, a **temporary replacement** is found (`ScheduleAssignmentReplacements`).
5. If a **student permanently drops out**, a new student is assigned or schedules are split.
6. Service continues, and payments are processed accordingly.

---

#### **Affected Tables:**
| Table | Purpose |
|-------|---------|
| `Orders` | Tracks the **recurring** service request. |
| `OrderSchedules` | Defines the specific days/times for all recurring sessions. |
| `ScheduleAssignments` | Tracks the **primary** student assigned to the recurring service. |
| `ScheduleAssignmentReplacements` | Logs **temporary** replacements if a student is unavailable. |
| `JobInstances` | Tracks execution for each scheduled session. |
| `PaymentTransactions` | Ensures each completed service is billed separately. |

---

## 🔄 **Use Case Flow: Student Substitution**
### **Scenario 1: Student is Sick for a Single Day**
#### **Flow:**
1. **Student notifies the system** they are unavailable for a specific day.
2. The system finds a **temporary replacement**:
   - Searches for another student who can provide the same service.
   - Prioritizes availability, location, and rating.
3. **New student is assigned** → (`ScheduleAssignmentReplacements` entry created).
4. **Service continues without disruption**.

#### **Affected Tables:**
| Table | Purpose |
|-------|---------|
| `ScheduleAssignments` | Original student assignment. |
| `ScheduleAssignmentReplacements` | Logs the temporary replacement. |
| `JobInstances` | Updates the new student’s involvement. |

---

### **Scenario 2: Student Drops Out of a Recurring Assignment**
#### **Flow:**
1. **Student notifies the system** they can no longer fulfill their recurring assignment.
2. The system attempts to **find a full replacement**:
   - Searches for another student who can **take over all remaining days**.
   - If no such student exists, the system **splits the assignment** among multiple students.
3. If a **new student is found**:
   - **ScheduleAssignments** is updated.
   - **Senior is notified** of the change.
4. If no replacement is available:
   - **Admin intervention is required**.
   - The senior may be asked to **reschedule or cancel**.

#### **Affected Tables:**
| Table | Purpose |
|-------|---------|
| `ScheduleAssignments` | Updates or reassigns the student for recurring service. |
| `ScheduleAssignmentReplacements` | Logs details of the transition process. |
| `JobInstances` | Ensures work sessions are still properly assigned. |

---

## 🔍 **Challenges & Solutions**
### **🚀 Challenge: Matching Seniors with Available Students**
#### Solution:
1. **Priority Matching**: Students who can handle all required days are given preference.
2. **Partial Fulfillment**: If no student covers all days, assignments are split.
3. **Emergency Substitutes**: If a student cancels, a replacement is found.

### **❌ Challenge: Seniors Selecting an Unavailable Location**
#### Solution:
1. **City Coverage Check**: Before an order is placed, the senior’s city is checked in `ServiceRegions`.
2. **Dynamic Expansion**: Admins can add new service regions as student coverage grows.

---

## 🔍 **Indexes & Performance Optimization**
- **`idx_user_type`**: Faster lookups for different user roles.
- **`idx_verification`**: Quick searches for pending student verifications.
- **`idx_payment_retries`**: Efficient tracking of failed payments for retries.
- **`idx_senior_relationship`**: Faster queries on seniors and their assigned customers.

---

## 📌 **Useful Queries for Each Actor (EF Core, .NET 8)**

### 🧑‍💼 **Admin Queries**
#### **1️⃣ Get All Pending Student Verifications**
```csharp
var pendingStudents = await _context.Students
    .Where(s => s.VerificationStatus == "pending")
    .Include(s => s.User)
    .Include(s => s.Faculty)
    .ToListAsync();
```

#### **2️⃣ Get All Active Orders in the System
```csharp
var activeOrders = await _context.Orders
    .Where(o => o.Status == "pending" || o.Status == "in_progress")
    .Include(o => o.Senior)
    .ThenInclude(s => s.Customer)
    .Include(o => o.Service)
    .ToListAsync();
```

#### **3️⃣ Get Payments Pending Processing
```csharp
var pendingPayments = await _context.PaymentTransactions
    .Where(pt => pt.Status == "pending" && pt.ScheduledAt <= DateTime.UtcNow)
    .Include(pt => pt.Customer)
    .Include(pt => pt.PaymentMethod)
    .ToListAsync();

```


###  🎓 **Student Queries**
#### **4️⃣ Get Student’s Assigned Services
```csharp
var studentServices = await _context.StudentServices
    .Where(ss => ss.StudentId == studentId)
    .Include(ss => ss.Service)
    .ToListAsync();

```
#### **5️⃣ Get Student’s Upcoming Jobs
```csharp
var upcomingJobs = await _context.JobInstances
    .Where(j => j.Status == "upcoming" && j.Assignment.StudentId == studentId)
    .Include(j => j.Assignment)
    .ThenInclude(a => a.OrderSchedule)
    .ToListAsync();

```
#### **6️⃣ Get Open Job Requests Sent to a Student
```csharp
var jobRequests = await _context.JobRequests
    .Where(jr => jr.StudentId == studentId && jr.Status == "pending")
    .Include(jr => jr.OrderSchedule)
    .Include(jr => jr.OrderSchedule.Order)
    .ThenInclude(o => o.Service)
    .ToListAsync();

```
#### **7️⃣ Get Student’s Reviews and Ratings
```csharp
var studentReviews = await _context.Reviews
    .Where(r => r.StudentId == studentId)
    .Include(r => r.Senior)
    .ToListAsync();

```

### 👵 **Senior Queries**
#### **8️⃣ Get Senior’s Active Orders
```csharp
var seniorOrders = await _context.Orders
    .Where(o => o.SeniorId == seniorId && (o.Status == "pending" || o.Status == "in_progress"))
    .Include(o => o.Service)
    .ToListAsync();

```
#### **9️⃣ Get Senior’s Order History
```csharp
var orderHistory = await _context.Orders
    .Where(o => o.SeniorId == seniorId && o.Status == "completed")
    .Include(o => o.Service)
    .ToListAsync();

```
#### **🔟 Get Seniors with Special Requirements
```csharp
var seniorsWithNeeds = await _context.Seniors
    .Where(s => s.SpecialRequirements != null)
    .Include(s => s.Customer)
    .Include(s => s.ContactInfo)
    .ToListAsync();

```

#### 🛍️  **Customer Queries**
#### ** 1️⃣1️⃣ Get Customer’s Preferred Payment Method
```csharp
var preferredPaymentMethod = await _context.PaymentMethods
    .Where(pm => pm.CustomerId == customerId && pm.IsDefault)
    .FirstOrDefaultAsync();

```
#### ** 1️⃣2️⃣ Get Customer’s Seniors
```csharp
var customerSeniors = await _context.Seniors
    .Where(s => s.CustomerId == customerId)
    .Include(s => s.ContactInfo)
    .ToListAsync();
```

#### ** 1️⃣3️⃣ Get Customer’s Invoices and Payment Status
```csharp
var customerInvoices = await _context.Invoices
    .Where(i => i.Transaction.CustomerId == customerId)
    .Include(i => i.Transaction)
    .ToListAsync();
```

#### 📅 **Order & Scheduling Queries**
#### ** 1️⃣4️⃣ Find Available Students for a Service
```csharp
var availableStudents = await _context.StudentAvailabilitySlots
    .Where(sas => sas.DayOfWeek == requestDay && 
                  sas.StartTime <= requestStartTime &&
                  sas.EndTime >= requestEndTime &&
                  _context.StudentServices.Any(ss => ss.StudentId == sas.StudentId && ss.ServiceId == requestServiceId))
    .Include(sas => sas.Student)
    .ToListAsync();
```

#### ** 1️⃣5️⃣ Get Order Schedule Details
```csharp
var orderSchedule = await _context.OrderSchedules
    .Where(os => os.OrderId == orderId)
    .Include(os => os.Order)
    .ThenInclude(o => o.Service)
    .ToListAsync();
```

#### ** 1️⃣6️⃣ Find Emergency Substitute for an Assignment
```csharp
var substituteStudent = await _context.Students
    .Where(s => _context.StudentAvailabilitySlots
        .Any(sas => sas.StudentId == s.Id && 
                    sas.DayOfWeek == affectedDay &&
                    sas.StartTime <= affectedStartTime &&
                    sas.EndTime >= affectedEndTime) &&
        _context.StudentServices.Any(ss => ss.StudentId == s.Id && ss.ServiceId == affectedServiceId))
    .OrderByDescending(s => s.AverageRating)
    .FirstOrDefaultAsync();
```

#### 💰 **Payment Queries**
#### ** 1️⃣7️⃣ Find Unpaid Invoices
```csharp
var unpaidInvoices = await _context.Invoices
    .Where(i => i.Status != "paid" && i.DueDate < DateTime.UtcNow)
    .ToListAsync();
```

#### ** 1️⃣8️⃣ Find Transactions with Failed Payments
```csharp
var failedPayments = await _context.PaymentTransactions
    .Where(pt => pt.Status == "failed" && pt.RetryCount < pt.MaxRetries)
    .ToListAsync();
```

#### ** 1️⃣9️⃣ Get Payment History for a Customer
```csharp
var paymentHistory = await _context.PaymentTransactions
    .Where(pt => pt.CustomerId == customerId)
    .Include(pt => pt.Invoice)
    .OrderByDescending(pt => pt.ProcessedAt)
    .ToListAsync();
```


####  **SCHEMA**
```SQL
-- CORE IDENTITY & AUTHENTICATION
CREATE TABLE Users (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_type ENUM('admin', 'student', 'customer') NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_user_type (user_type)
);

-- SHARED CONTACT INFORMATION
CREATE TABLE ContactInfo (
    id INT PRIMARY KEY AUTO_INCREMENT,
    first_name VARCHAR(255) NOT NULL,
    last_name VARCHAR(255) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    gender ENUM('Male', 'Female') NOT NULL,
    
    -- Location Fields 
    google_place_id VARCHAR(255) NOT NULL,  
    full_address TEXT NOT NULL,
    city_id INT NOT NULL,  
    latitude DECIMAL(10, 8) NOT NULL,
    longitude DECIMAL(11, 8) NOT NULL,
    
  
    city VARCHAR(100),
    state CHAR(2),
    postal_code VARCHAR(20),
    country CHAR(2) DEFAULT 'US',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_contact_search (last_name, first_name),
    FOREIGN KEY (city_id) REFERENCES Cities(id),
);
-- SERVICE PROVIDERS (STUDENTS)
CREATE TABLE Students (
    UserId INT PRIMARY KEY,
    student_number VARCHAR(20) NOT NULL UNIQUE,  
    faculty_id INT NOT NULL,              
    date_registered DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    contact_id INT NOT NULL,
    verification_status ENUM('pending', 'verified', 'suspended') DEFAULT 'pending',
    background_check_date DATE,
    average_rating DECIMAL(3,2) DEFAULT 0.00,
    FOREIGN KEY (id) REFERENCES Users(id) ON DELETE CASCADE,
    FOREIGN KEY (contact_id) REFERENCES ContactInfo(id),
    FOREIGN KEY (faculty_id) REFERENCES Faculties(id),
    INDEX idx_verification (verification_status)
);

CREATE TABLE Faculties (
    id INT PRIMARY KEY AUTO_INCREMENT,
    faculty_name VARCHAR(100) NOT NULL UNIQUE
);

-- LEGAL DOCUMENT STORAGE
CREATE TABLE StudentContracts (
    id INT PRIMARY KEY AUTO_INCREMENT,
    student_id INT NOT NULL,
    cloud_path VARCHAR(512) NOT NULL,
    effective_date DATE NOT NULL,
    expiration_date DATE,
    uploaded_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (student_id) REFERENCES Students(UserId),
    INDEX idx_contract_expiry (expiration_date)
);

-- SERVICE CONSUMERS
CREATE TABLE Customers (
    UserId INT PRIMARY KEY,
    contact_id INT NOT NULL,
    preferred_notification_method ENUM('email', 'sms', 'push') DEFAULT 'email',
    FOREIGN KEY (id) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (contact_id) REFERENCES ContactInfo(id)
);

-- SERVICE RECIPIENTS (SENIORS)
CREATE TABLE Seniors (
    id INT PRIMARY KEY AUTO_INCREMENT,
    customer_id INT NOT NULL,
    contact_id INT NOT NULL,
    relationship ENUM('self', 'spouse', 'parent', 'relative', 'other') NOT NULL,
    special_requirements JSON,
    FOREIGN KEY (customer_id) REFERENCES Customers(UserId),
    FOREIGN KEY (contact_id) REFERENCES ContactInfo(id),
    INDEX idx_senior_relationship (customer_id, relationship)
);

-- PAYMENT SYSTEM
CREATE TABLE PaymentMethods (
    id INT PRIMARY KEY AUTO_INCREMENT,
    customer_id INT NOT NULL,
    processor ENUM('stripe', 'paypal') NOT NULL,
    token VARCHAR(255) NOT NULL,
    is_default BOOLEAN DEFAULT FALSE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (customer_id) REFERENCES Customers(UserId),
    INDEX idx_default_payment (customer_id, is_default)
);

-- SERVICE CATALOG
CREATE TABLE ServiceCategories (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(255) UNIQUE NOT NULL,
    icon VARCHAR(50),
    INDEX idx_category_name (name)
);

CREATE TABLE Services (
    id INT PRIMARY KEY AUTO_INCREMENT,
    category_id INT NOT NULL,
    name VARCHAR(255) NOT NULL,
    base_price DECIMAL(10,2) NOT NULL,
    min_duration SMALLINT NOT NULL,
    FOREIGN KEY (category_id) REFERENCES ServiceCategories(id),
    INDEX idx_service_pricing (base_price)
);

-- STUDENT SERVICE OFFERINGS
CREATE TABLE StudentServices (
    student_id INT NOT NULL,
    service_id INT NOT NULL,
    experience_years TINYINT,
    PRIMARY KEY (student_id, service_id),
    FOREIGN KEY (student_id) REFERENCES Students(UserId),
    FOREIGN KEY (service_id) REFERENCES Services(id)
);

-- AVAILABILITY MANAGEMENT
CREATE TABLE StudentAvailabilitySlots (
    student_id INT NOT NULL,
    day_of_week TINYINT NOT NULL CHECK (0 <= day_of_week <= 6),
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
     PRIMARY KEY (student_id, day_of_week),
    FOREIGN KEY (student_id) REFERENCES Students(UserId),
    CONSTRAINT CHK_ValidSlot CHECK (start_time < end_time),
    INDEX idx_availability_slots (student_id, day_of_week)
);

-- Order Management
CREATE TABLE Orders (
    id INT PRIMARY KEY AUTO_INCREMENT,
    senior_id INT NOT NULL,
    service_id INT NOT NULL,
    status ENUM('pending', 'accepted', 'in_progress', 'completed', 'cancelled') NOT NULL DEFAULT 'pending',
    is_recurring BOOLEAN NOT NULL,
    recurrence_pattern ENUM('daily', 'weekly', 'biweekly', 'monthly'),
    start_date DATE,
    end_date DATE,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (senior_id) REFERENCES Seniors(id) ON DELETE CASCADE,
    FOREIGN KEY (service_id) REFERENCES Services(id) ON DELETE CASCADE
);

-- Scheduling System
CREATE TABLE OrderSchedules (
    id INT PRIMARY KEY AUTO_INCREMENT,
    order_id INT NOT NULL,
    day_of_week NOT TINYINT,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    is_cancelled BOOLEAN DEFAULT FALSE,
    cancellation_reason TEXT,
    FOREIGN KEY (order_id) REFERENCES Orders(id) ON DELETE CASCADE
);

-- Matching System
CREATE TABLE JobRequests (
    id INT PRIMARY KEY AUTO_INCREMENT,
    order_schedule_id INT NOT NULL,
    student_id INT NOT NULL,
    status ENUM('pending', 'accepted', 'declined', 'expired') NOT NULL DEFAULT 'pending',
    sent_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    expires_at DATETIME NOT NULL,
    is_emergency_sub BOOLEAN NOT NULL DEFAULT FALSE,
    priority_level TINYINT DEFAULT 1,
    FOREIGN KEY (order_schedule_id) REFERENCES OrderSchedules(id) ON DELETE CASCADE,
    FOREIGN KEY (student_id) REFERENCES Students(UserId) ON DELETE CASCADE
);

-- Assignment Tracking
CREATE TABLE ScheduleAssignments (
    id INT PRIMARY KEY AUTO_INCREMENT,
    order_schedule_id INT NOT NULL,
    student_id INT NOT NULL,
    status ENUM('pending', 'accepted', 'declined','canceled', 'completed') NOT NULL DEFAULT 'pending',
    is_temporary BOOLEAN NOT NULL DEFAULT FALSE,
    termination_reason ENUM('completed', 'student_quit', 'senior_canceled', 'system_terminated'),
    terminated_at DATETIME,
    assigned_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    accepted_at DATETIME,
    completed_at DATETIME,
    FOREIGN KEY (order_schedule_id) REFERENCES OrderSchedules(id) ON DELETE CASCADE,
    FOREIGN KEY (student_id) REFERENCES Students(id) ON DELETE CASCADE
);

-- Service Execution Tracking
CREATE TABLE JobInstances (
    id INT PRIMARY KEY AUTO_INCREMENT,
    assignment_id INT NOT NULL,
    original_assignment_id INT NULL,
    scheduled_date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    status ENUM('upcoming', 'in_progress', 'completed', 'cancelled') NOT NULL DEFAULT 'upcoming',
    substitution_status ENUM('original', 'needs_substitute', 'substituted') NOT NULL DEFAULT 'original',
    actual_start_time DATETIME,
    actual_end_time DATETIME,
    FOREIGN KEY (assignment_id) REFERENCES ScheduleAssignments(id) ON DELETE CASCADE,
    FOREIGN KEY (original_assignment_id) REFERENCES ScheduleAssignments(id) ON DELETE SET NULL
);

-- PAYMENT PROCESSING SYSTEM
CREATE TABLE PaymentTransactions (
    id INT PRIMARY KEY AUTO_INCREMENT,
    job_instance_id INT NOT NULL,
    customer_id INT NOT NULL,
    payment_method_id INT NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    currency CHAR(3) NOT NULL DEFAULT 'USD',
    
    -- Payment timing controls
    scheduled_at DATETIME NOT NULL,  -- Always: job_instance.start_time - 10 minutes
    processed_at DATETIME,           -- Actual processing time
    next_retry_at DATETIME,
    
    -- Transaction state
    status ENUM('pending', 'processing', 'succeeded', 'failed', 'refunded') DEFAULT 'pending',
    retry_count TINYINT DEFAULT 0,
    max_retries TINYINT DEFAULT 3,
    
    -- Gateway integration
    gateway_id VARCHAR(255),
    gateway_response JSON,
    idempotency_key CHAR(64) UNIQUE,
    
    FOREIGN KEY (job_instance_id) REFERENCES JobInstances(id),
    FOREIGN KEY (customer_id) REFERENCES Customers(id),
    FOREIGN KEY (payment_method_id) REFERENCES PaymentMethods(id)
    
    INDEX idx_pending_payment (status, scheduled_at),
    INDEX idx_payment_retries (next_retry_at, retry_count)
);

-- Replacement History
CREATE TABLE ScheduleAssignmentReplacements (
    id INT PRIMARY KEY AUTO_INCREMENT,
    original_assignment_id INT NOT NULL,
    new_assignment_id INT NOT NULL,
    replaced_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    replacement_reason TEXT,
    initiated_by ENUM('system', 'senior', 'student'),
    FOREIGN KEY (original_assignment_id) REFERENCES ScheduleAssignments(id),
    FOREIGN KEY (new_assignment_id) REFERENCES ScheduleAssignments(id)
);

-- Feedback System
CREATE TABLE Reviews (
    id INT PRIMARY KEY AUTO_INCREMENT,
    senior_id INT NOT NULL,
    student_id INT NOT NULL,
    job_instance_id INT NOT NULL,
    rating TINYINT NOT NULL CHECK (rating BETWEEN 1 AND 5),
    comment TEXT,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (senior_id) REFERENCES Seniors(id) ON DELETE CASCADE,
    FOREIGN KEY (student_id) REFERENCES Students(UserId) ON DELETE CASCADE,
    FOREIGN KEY (job_instance_id) REFERENCES JobInstances(id) ON DELETE CASCADE
);

-- INVOICE SYSTEM ENHANCEMENTS
CREATE TABLE Invoices (
    id INT PRIMARY KEY AUTO_INCREMENT,
    job_instance_id INT NOT NULL,
    transaction_id INT NOT NULL,
    mailerlite_campaign_id VARCHAR(255),
    invoice_number VARCHAR(50) NOT NULL UNIQUE,
    status ENUM('pending', 'sent', 'viewed', 'paid', 'overdue') DEFAULT 'pending',
    due_date DATE NOT NULL,
    sent_at DATETIME,
    viewed_at DATETIME,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (job_instance_id) REFERENCES JobInstances(id),
    FOREIGN KEY (transaction_id) REFERENCES PaymentTransactions(id)
);




-- TRACK EMAIL INTEGRATION
CREATE TABLE InvoiceEmails (
    id INT PRIMARY KEY AUTO_INCREMENT,
    invoice_id INT NOT NULL,
    mailerlite_message_id VARCHAR(255),
    status ENUM('queued', 'sent', 'delivered', 'opened', 'failed'),
    opened_count INT DEFAULT 0,
    last_attempt DATETIME,
    attempt_count INT DEFAULT 0,
    error_message TEXT,
    FOREIGN KEY (invoice_id) REFERENCES Invoices(id),
    INDEX idx_email_tracking (mailerlite_message_id)
);

-- SERVICE AREA MANAGEMENT
CREATE TABLE Cities (
    id INT PRIMARY KEY AUTO_INCREMENT,
    google_place_id VARCHAR(255) NOT NULL UNIQUE,  -- From Google Places API
    official_name VARCHAR(255) NOT NULL,
    bounds POLYGON NOT NULL,  -- Geospatial boundaries
    is_serviced BOOLEAN NOT NULL DEFAULT FALSE,
    added_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    SPATIAL INDEX(bounds)
);


-- SERVICE AVAILABILITY TRACKING
CREATE TABLE ServiceRegions (
    id INT PRIMARY KEY AUTO_INCREMENT,
    city_id INT NOT NULL,
    service_id INT NOT NULL,
    active BOOLEAN NOT NULL DEFAULT TRUE,
    coverage_radius_km INT NOT NULL,  -- Max distance from city center
    FOREIGN KEY (city_id) REFERENCES Cities(id),
    FOREIGN KEY (service_id) REFERENCES Services(id)
);







```