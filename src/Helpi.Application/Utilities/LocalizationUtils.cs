

using Helpi.Application.Common.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Utilities
{
    public static class LocalizationUtils
    {
        public static string GetEntityDescription(ILocalizationService localizer,
        int orderId,
        int scheduleId,
        int? jobInstanceId,
         string culture)
        {

            var orderStr = localizer.GetString("Entities.Order", culture, orderId);
            var scheduleStr = localizer.GetString("Entities.Schedule", culture, scheduleId);

            string jobStr = "";

            if (jobInstanceId.HasValue)
            {
                // Use a localized template like "Job #{0}"
                jobStr = "| " + localizer.GetString("Entities.Job", culture, jobInstanceId.Value);
            }

            return $"{orderStr} | {scheduleStr} {jobStr}";

        }

    }
}
