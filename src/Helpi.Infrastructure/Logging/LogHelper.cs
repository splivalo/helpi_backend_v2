using Microsoft.Extensions.Logging;

namespace Helpi.Infrastructure.Logging;
public static class LogHelper
{
    public static void LogError(ILogger logger, Exception ex, string message, int id)
    {
        logger.LogError(ex, "❌ {message} - ID: {id}", message, id);
    }

    //    public static void LogInformation(ILogger logger, int id)
    // {
    //     logger.LogInformation("🔄 [MATCHING] Initiating match process for OrderId: {OrderId}", orderId);
    // }

    // public static void LogDomainError(ILogger logger, Exception ex, string context)
    // {
    //     logger.LogError(ex, "💥 [DOMAIN ERROR] An error occurred in {Context}", context);
    // }




}