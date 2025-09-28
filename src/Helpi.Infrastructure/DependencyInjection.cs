
using System.Text;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Events;
using Helpi.Infrastructure.BackgroundJobs.Jobs;
using Helpi.Infrastructure.Payment.Stripe;
using Helpi.Infrastructure.Persistence;
using Helpi.Infrastructure.Repositories;
using Helpi.Infrastructure.Seeds;
using Helpi.Infrastructure.Services;
using Infrastructure.Persistence.Repositories;
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

        // Register DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                o => o.UseNetTopologySuite()), ServiceLifetime.Scoped);



        services.AddSingleton<IEventMediator, EventMediator>();



        services.AddScoped<IHNotificationRepository, HNotificationRepository>();

        services.AddHttpClient<IApiService, ApiService>();


        services.AddScoped<IGooglePlaceService, GooglePlaceService>();
        services.AddScoped<IMinimaxService, MinimaxService>();
        services.AddScoped<IPaymentErrorMapper, StripeErrorMapper>();
        services.AddScoped<IPricingChangeHistoryRepository, PricingChangeHistoryRepository>();
        services.AddScoped<IPricingConfigurationRepository, PricingConfigurationRepository>();
        services.AddScoped<IPaymentProfileRepository, PaymentProfileRepository>();
        services.AddScoped<IContractNumberSequenceRepository, ContractNumberSequenceRepository>();

        services.AddScoped<IFcmTokensRepository, FcmTokensRepository>();
        // services.AddScoped<IMailerLiteService, MailerLiteService>();
        services.AddHttpClient<IMailerLiteService, MailerLiteService>();
        services.AddScoped<IFirebaseService, FirebaseService>();
        services.AddScoped<IMatchingBackgroundJobs, MatchingBackgroundJobs>();
        services.AddScoped<IJobInstanceJobs, JobInstanceJobs>();
        services.AddScoped<StudentBackgroundJobs>();
        services.AddScoped<IHangfireService, HangfireService>();
        services.AddScoped<OrderStatusMaintenanceService>();

        services.AddScoped<IReassignmentService, ReassignmentService>();

        // Register all repositories
        services.AddScoped<IReassignmentRecordRepository, ReassignmentRecordRepository>();
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
        // services.AddScoped<IScheduleAssignmentReplacementRepository, ScheduleAssignmentReplacementRepository>();
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


        services.AddTransient<RoleSeeder>();
        services.AddTransient<ContractNumberSequenceSeeder>();
        services.AddTransient<ServiceDataSeeder>();

        return services;
    }

    public static IServiceCollection AddGoogleDriveServices(
      this IServiceCollection services, IConfiguration configuration)
    {

        var googleCredentialsPath = Environment.GetEnvironmentVariable("GOOGLE_DRIVE_CREDENTIALS_JSON")
                            ?? configuration["GoogleDrive:CredentialsJson"];

        if (string.IsNullOrEmpty(googleCredentialsPath))
        {
            throw new InvalidOperationException("Google Drive credentials not found in environment variables.");
        }

        if (!File.Exists(googleCredentialsPath))
        {
            throw new FileNotFoundException($"Google Drive credentials file not found at {googleCredentialsPath}");
        }

        var googleCredentialsJson = File.ReadAllText(googleCredentialsPath);

        services.Configure<GoogleDriveSettings>(options =>
        {
            options.ApplicationName = configuration["GoogleDrive:ApplicationName"]
                ?? throw new InvalidOperationException("GoogleDrive:ApplicationName configuration is missing");

            options.BaseFolderId = configuration["GoogleDrive:BaseFolderId"]
                ?? throw new InvalidOperationException("GoogleDrive:BaseFolderId configuration is missing");

            options.CredentialsJson = googleCredentialsJson; // now it's the actual JSON
        });


        services.AddSingleton<IGoogleDriveService, GoogleDriveService>();

        return services;
    }
    public static IServiceCollection AddSignalRServices(
      this IServiceCollection services, IConfiguration configuration)
    {

        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}