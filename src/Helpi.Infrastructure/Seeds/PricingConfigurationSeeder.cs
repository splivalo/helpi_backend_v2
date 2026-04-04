using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Helpi.Infrastructure.Seeds;

public static class PricingConfigurationSeeder
{
    public static async Task SeedPriceConfigAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!await db.PricingConfigurations.AnyAsync())
        {
            db.PricingConfigurations.Add(new PricingConfiguration
            {
                Id = 1,
                JobHourlyRate = 14,
                SundayHourlyRate = 21,
                CompanyPercentage = 40,
                ServiceProviderPercentage = 60,
                StudentCancelCutoffHours = 6,
                SeniorCancelCutoffHours = 1,
                TravelBufferMinutes = 15,
                PaymentTimingMinutes = 30,
                VatEnabled = false,
                VatPercentage = 0
            });

            await db.SaveChangesAsync();
        }
    }
}
