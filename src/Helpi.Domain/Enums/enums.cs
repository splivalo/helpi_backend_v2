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
        UnVerified,
        Verified,
        ContractRenewalNeeded,
        AccountDeactivated,  // Account soft-deleted after 3 months non-renewal
        PendingPermanentDeletion,  // Warned, awaiting 6-month mark for hard delete
        Deleted
    }

    public enum ContractStatus
    {
        valid,
        expired
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
        Pending,
        Acitve,
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
        AssignedToOther
    }

    public enum AssignmentStatus
    {
        Accepted,
        Declined,
        Canceled,
        Completed
    }

    public enum TerminationReason
    {
        Completed,
        StudentQuit,
        SeniorCanceled,
        SystemTerminated
    }

    public enum JobInstanceStatus
    {
        Upcoming,
        InProgress,
        Completed,
        Cancelled
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

    public enum Gender
    {
        Male,
        Female
    }

    public enum NotificationType
    {
        General,
        JobRequest,
        AssignmentConfirmed,
        ScheduleChange,
        PaymentReceipt,
        JobInProgress,
        ContractRenewalRequired,
        contractValid

    }
}