namespace Helpi.Application.DTOs
{
    using Helpi.Domain.Entities;

    public sealed record ContractEvaluationResult(
        StudentContract? ActiveContract,
        StudentContract? NextContract,
        /// <summary>True when there is at least one-day gap between active and next (or there is no active/next) — meaning expiry actions may be appropriate.</summary>
        bool HasGap,
        /// <summary>Days until the active contract expires (if any)</summary>
        int? DaysUntilExpiry,
        /// <summary>Days since last expiration (if active is null and there is a last contract)</summary>
        int? DaysSinceExpiry
    );
}
