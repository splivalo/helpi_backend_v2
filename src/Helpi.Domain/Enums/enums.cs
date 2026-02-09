namespace Helpi.Domain.Enums
{
    public enum UserType
    {
        Admin,
        Student,
        Customer
    }

    public enum StudentStatus
    {
        InActive,
        Active,
        ContractAboutToExpire,
        Expired,
        AccountDeactivated,  // Account soft-deleted after 3 months non-renewal
        PendingPermanentDeletion,  // Warned, awaiting 6-month mark for hard delete
        Deleted
    }

    public enum ContractStatus
    {
        Pending,    // Exists but not yet effective
        Active,     // Currently valid
        Expired     // Was valid but has ended
    }

    public enum Relationship
    {
        Self,
        Spouse,
        Parent,
        Relative,
        Other
    }

    public enum NotificationMethod
    {
        Email,
        Sms,
        Push
    }

    public enum PaymentProcessor
    {
        Stripe,
        Paypal
    }

    public enum OrderStatus
    {
        InActive,
        Pending,
        FullAssigned,
        Completed,
        Cancelled
    }


    public enum RecurrencePattern
    {
        Weekly,
    }

    public enum JobRequestStatus
    {
        Pending,
        Accepted,
        Declined,
        AssignedToOther,
        Cancelled
    }

    public enum ReassignmentTrigger
    {
        ContractExpiration,
        StudentRequest,
        SeniorRequest,
        AdminIntervention,
    }

    public enum ReassignmentStatus
    {
        Requested,
        InProgress,
        Assigned,
        AllEligableStudentNotified,
        NoEligibleStudents,
        MaxAttemptsReached,
        Cancelled,

    }

    public enum ReassignmentType
    {
        OneDaySubstitution,    // Reassigning a single day/instance
        CompleteTakeover,      // Reassigning the entire schedule
    }

    public enum AssignmentStatus
    {
        Accepted,
        Declined,
        Terminated,
        Completed,

    }

    public enum TerminationReason
    {
        Completed,
        StudentQuit,
        StudentRequested,
        StudentContractExpired,
        SeniorCanceled,
        SystemTerminated,
        AdminIntervention
    }

    public enum JobInstanceStatus
    {
        Upcoming,
        InProgress,
        Completed,
        Cancelled,
        Rescheduled

    }

    public enum SubstitutionStatus
    {
        Original,
        NeedsSubstitute,
        Substituted
    }

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Paid,
        Failed,
        Refunded
    }

    public enum ReplacementInitiator
    {
        System,
        Senior,
        Student
    }

    public enum InvoiceStatus
    {
        Pending,
        Sent,
        Viewed,
        Paid,
        Overdue
    }

    public enum EmailStatus
    {
        Queued,
        Sent,
        Delivered,
        Opened,
        Failed
    }
    public enum EmailType
    {
        Invoice
    }

    public enum Gender
    {
        Male,
        Female
    }

    public enum NotificationType
    {
        General,
        PaymentSuccess,
        PaymentFailed,
        PaymentRefunded,
        JobRequest,
        JobStartReminder,
        JobInProgress,
        JobCompleted,
        JobCancelled,
        JobRescheduled,
        ScheduleAssignmentCancelled,
        OrderScheduleCancelled,
        OrderCancelled,
        ContractAboutToExpire,
        ContractExpired,
        ContractAdded,
        ContractUpdated,
        ContractDeleted,
        AllEligableStudentNotified,
        NoEligibleStudents,
        MatchingMaxAttemptsReached,
        ReviewRequest,
        ReassignmentStarted,
        ReassignmentCompleted,

        NewStudentAdded,
        NewSeniorAdded,
        StudentDeleted,
        SeniorDeleted,
        CustomerDeleted,
        AdminDeleted,

    }

    public enum DashboardTileType
    {
        uncoveredOrders,
        expiredContracts,
        newNotifications,
        newMessages,
        studentCount,
        invalidContracts,
        reviewCount,
        averageReview,
        userCount,
        orderCount,
        completedOrders,
        workedHours
    }

    public enum ChangeType
    {
        increased,
        decreased,
        remained
    }

    public enum AutoScheduleDisableReason
    {
        admin,
        noEligibleStudents,
        allEligibleStudentsNotified,
    }

    public enum DevicePlatform
    {
        Android,
        iOS
    }

}