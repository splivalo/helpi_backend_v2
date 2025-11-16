using System.Linq;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services
{
    public class ContractEvaluationService : IContractEvaluationService
    {
        public ContractEvaluationResult Evaluate(Student student)
        {
            if (student == null) throw new ArgumentNullException(nameof(student));

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Ensure deterministic ordering by EffectiveDate ascending
            var ordered = student.Contracts
                .OrderBy(c => c.EffectiveDate)
                .ToList();

            // Active: currently in effect
            var active = ordered.FirstOrDefault(c =>
                c.EffectiveDate <= today && c.ExpirationDate >= today);

            // Next: the earliest contract that starts AFTER today
            var next = ordered.FirstOrDefault(c => c.EffectiveDate > today);

            int? daysUntilExpiry = null;
            int? daysSinceExpiry = null;
            bool hasGap = true; // optimistic default

            if (active != null)
            {
                daysUntilExpiry = active.ExpirationDate.DayNumber - today.DayNumber;

                if (next != null)
                {
                    // gap is number of days from active expiration to next effective
                    var gap = next.EffectiveDate.DayNumber - active.ExpirationDate.DayNumber;

                    // if gap <= 1 then there is no meaningful gap (i.e., immediate continuation / contiguous)
                    // Example: active expires 2025-03-10, next starts 2025-03-11 -> gap == 1 -> treat as no gap
                    hasGap = gap > 1;
                }
                else
                {
                    // no next contract => gap true (may be expired soon)
                    hasGap = true;
                }
            }
            else if (next != null)
            {
                // No active contract but a next contract exists in future
                // If next starts today or tomorrow (gap <=1) it's effectively a smooth transition
                var gap = next.EffectiveDate.DayNumber - today.DayNumber;
                hasGap = gap > 1;
            }
            else
            {
                // No contracts that are active or pending -> find last contract to compute daysSinceExpiry
                var last = ordered.LastOrDefault();
                if (last != null)
                {
                    daysSinceExpiry = today.DayNumber - last.ExpirationDate.DayNumber;
                }

                hasGap = true;
            }

            return new ContractEvaluationResult(active, next, hasGap, daysUntilExpiry, daysSinceExpiry);
        }
    }
}
