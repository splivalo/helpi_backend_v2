
using System.Text;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.BackgroundJobs.Jobs;
using Helpi.Infrastructure.Persistence;
using Helpi.Infrastructure.Repositories;
using Helpi.Infrastructure.Seeds;
using Helpi.Infrastructure.Services;
using MailerLiteIntegration.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
                o => o.UseNetTopologySuite()), ServiceLifetime.Scoped);





        services.AddScoped<IMailerLiteService, MailerLiteService>();
        services.AddHttpClient<MailerLiteService>();
        services.AddScoped<IFirebaseService, FirebaseService>();
        services.AddScoped<IMatchingBackgroundJobs, MatchingBackgroundJobs>();
        services.AddScoped<IJobInstanceJobs, JobInstanceJobs>();

        // Register all repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthRepository, AuthRepository>();
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
        services.AddScoped<IOrderServiceRepository, OrderServiceRepository>();
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

        /// NOTE TO SELF: Alternative Approach - Automatic Registration (For large number of repositories):
        //         var repositoryTypes = Assembly.GetAssembly(typeof(AppDbContext))!
        //     .GetTypes()
        //     .Where(t => t.IsClass && 
        //                !t.IsAbstract && 
        //                t.Name.EndsWith("Repository"));

        // foreach (var repoType in repositoryTypes)
        // {
        //     var interfaceType = repoType.GetInterfaces()
        //         .First(i => i.Name == $"I{repoType.Name}");

        //     services.AddScoped(interfaceType, repoType);
        // }

        return services;
    }




    public static IServiceCollection AddIdentityServices(
          this IServiceCollection services, IConfiguration configuration)
    {


        services.AddIdentity<User, IdentityRole<int>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();



        var secretKey = configuration["JwtSettings:Secret"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT Secret Key is missing in configuration.");
        }

        var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
        var IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes);

        services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudience = configuration["JwtSettings:Audience"],
                    IssuerSigningKey = IssuerSigningKey
                };
            });

        services.AddAuthorization();

        // TODO: seeders
        services.AddTransient<RoleSeeder>();
        services.AddTransient<CitySeeder>();
        services.AddTransient<ServiceDataSeeder>();

        return services;
    }
}