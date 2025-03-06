using Helpi.Application.Interfaces;
using Helpi.Infrastructure.Persistence;
using Helpi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // services.AddDbContext<AppDbContext>(options =>
        //     options.UseNpgsql(
        //         configuration.GetConnectionString("Default"),
        //         b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Register DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                o => o.UseNetTopologySuite()));

        // Register all repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IContactInfoRepository, ContactInfoRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IFacultyRepository, FacultyRepository>();
        services.AddScoped<IStudentContractRepository, StudentContractRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISeniorRepository, SeniorRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IStudentServiceRepository, StudentServiceRepository>();
        services.AddScoped<IStudentAvailabilitySlotRepository, StudentAvailabilitySlotRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderScheduleRepository, OrderScheduleRepository>();
        services.AddScoped<IJobRequestRepository, JobRequestRepository>();
        services.AddScoped<IScheduleAssignmentRepository, ScheduleAssignmentRepository>();
        services.AddScoped<IJobInstanceRepository, JobInstanceRepository>();
        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services.AddScoped<IScheduleAssignmentReplacementRepository, ScheduleAssignmentReplacementRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceEmailRepository, InvoiceEmailRepository>();
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IServiceRegionRepository, ServiceRegionRepository>();

        return services;
    }
}