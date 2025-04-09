
using Helpi.Application.Common.Mappings;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Helpi.Application;


public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        // Register all services
        services.AddScoped<AuthService>();
        services.AddScoped<FcmTokensService>();
        services.AddScoped<RecurringJobService>();
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
        services.AddScoped<ServiceCategoryService>();
        services.AddScoped<ServiceService>();
        services.AddScoped<StudentServiceService>();
        services.AddScoped<StudentAvailabilitySlotService>();
        services.AddScoped<OrdersService>();
        services.AddScoped<OrderScheduleService>();
        services.AddScoped<JobRequestService>();
        services.AddScoped<ScheduleAssignmentService>();
        services.AddScoped<JobInstanceService>();
        services.AddScoped<PaymentTransactionService>();
        services.AddScoped<ScheduleAssignmentReplacementService>();
        services.AddScoped<ReviewService>();
        services.AddScoped<InvoiceService>();
        services.AddScoped<InvoiceEmailService>();
        services.AddScoped<CityService>();
        services.AddScoped<ServiceRegionService>();

        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        return services;
    }

}