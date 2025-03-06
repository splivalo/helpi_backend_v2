using System.ComponentModel.DataAnnotations.Schema;
using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class ScheduleAssignmentReplacement
    {
        public int Id { get; set; }
        public int OriginalAssignmentId { get; set; }
        public int NewAssignmentId { get; set; }
        public DateTime ReplacedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "text")]
        public string? ReplacementReason { get; set; }
        public ReplacementInitiator InitiatedBy { get; set; }

        public ScheduleAssignment OriginalAssignment { get; set; } = null!;
        public ScheduleAssignment NewAssignment { get; set; } = null!;
    }
}
