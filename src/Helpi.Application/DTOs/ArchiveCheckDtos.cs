namespace Helpi.Application.DTOs;

/// <summary>
/// Response DTO for archive/delete eligibility check.
/// Used for students, seniors, orders, and contracts.
/// </summary>
public class ArchiveCheckDto
{
    /// <summary>
    /// Whether the entity can be archived/deleted.
    /// If false and HasBlockingItems is true, a confirmation modal should be shown.
    /// </summary>
    public bool CanArchiveDirectly { get; set; }

    /// <summary>
    /// Whether there are active items that would be affected.
    /// </summary>
    public bool HasBlockingItems { get; set; }

    /// <summary>
    /// Number of active assignments (for students).
    /// </summary>
    public int ActiveAssignmentsCount { get; set; }

    /// <summary>
    /// Number of upcoming job instances/sessions.
    /// </summary>
    public int UpcomingSessionsCount { get; set; }

    /// <summary>
    /// Number of active orders (for seniors).
    /// </summary>
    public int ActiveOrdersCount { get; set; }

    /// <summary>
    /// Human-readable message for the confirmation modal.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for archive operation with force flag.
/// When Force is true, auto-cancels blocking items before archiving.
/// </summary>
public class ArchiveRequestDto
{
    /// <summary>
    /// If true, automatically cancel all blocking items and proceed with archive.
    /// If false and there are blocking items, the operation will fail.
    /// </summary>
    public bool Force { get; set; } = false;

    /// <summary>
    /// Optional reason for archiving.
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Response DTO for archive operation result.
/// </summary>
public class ArchiveResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Number of assignments that were terminated.
    /// </summary>
    public int TerminatedAssignmentsCount { get; set; }

    /// <summary>
    /// Number of sessions/job instances that were cancelled.
    /// </summary>
    public int CancelledSessionsCount { get; set; }

    /// <summary>
    /// Number of orders that were cancelled.
    /// </summary>
    public int CancelledOrdersCount { get; set; }
}
