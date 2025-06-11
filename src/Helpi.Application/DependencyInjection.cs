
using Helpi.Application.Common.Mappings;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Helpi.Application;


public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {

        ConfigureStripe(configuration);
        // Register all services

        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<StudentStatusService>();
        services.AddScoped<IStripePaymentService, StripePaymentService>();
        services.AddScoped<IContractNumberService, ContractNumberService>();
        services.AddScoped<AuthService>();
        services.AddScoped<FcmTokensService>();
        services.AddScoped<IHangfireRecurringJobService, HangfireRecurringJobService>();
        services.AddScoped<IRecurrenceDateGenerator, RecurrenceDateGenerator>();
        services.AddScoped<IMatchingService, MatchingService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<UserService>();
        services.AddScoped<ContactInfoService>();
        services.AddScoped<StudentService>();
        services.AddScoped<FacultyService>();
        services.AddScoped<StudentContractService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<SeniorService>();
        services.AddScoped<PaymentMethodService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ServiceCategoryService>();
        services.AddScoped<ServiceService>();
        services.AddScoped<StudentServiceService>();
        services.AddScoped<StudentAvailabilitySlotService>();
        services.AddScoped<OrdersService>();
        services.AddScoped<OrderScheduleService>();
        services.AddScoped<JobRequestService>();
        services.AddScoped<ScheduleAssignmentService>();
        services.AddScoped<IJobInstanceService, JobInstanceService>();
        services.AddScoped<PaymentTransactionService>();
        services.AddScoped<ScheduleAssignmentReplacementService>();
        services.AddScoped<ReviewService>();
        services.AddScoped<InvoiceService>();
        services.AddScoped<InvoiceEmailService>();
        services.AddScoped<CityService>();
        services.AddScoped<ServiceRegionService>();
        services.AddScoped<PricingConfigurationService>();
        services.AddScoped<PricingChangeHistoryService>();

        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        return services;
    }

    private static void ConfigureStripe(IConfiguration configuration)
    {
        var stripeSecretKey = Environment.GetEnvironmentVariable("Stripe:SecretKey")
            ?? configuration["Stripe:SecretKey"];

        if (string.IsNullOrWhiteSpace(stripeSecretKey))
        {
            throw new InvalidOperationException("Stripe Secret Key is not configured.");
        }

        Stripe.StripeConfiguration.ApiKey = stripeSecretKey;
    }

}
