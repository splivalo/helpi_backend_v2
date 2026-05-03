
using System.Security.Claims;
using System.Text;
using Helpi.Application.Common.Interfaces;
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
using MyApp.Infrastructure.Localization;

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
        npgsqlOptions =>
        {
            // Your existing NetTopologySuite
            npgsqlOptions.UseNetTopologySuite();

            // Add retry logic for transient failures
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null
            );

            // Command timeout (30 seconds)
            npgsqlOptions.CommandTimeout(30);

        }
    )
    );






        services.AddSingleton<ILocalizationService>(new JsonLocalizationService("en"));

        services.AddSingleton<IEventMediator, EventMediator>();

        services.AddScoped<INotificationFactory, NotificationFactory>();

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
        services.AddHttpClient<IMailerLiteService, MailerLiteService>();
        services.AddScoped<IMailgunService, MailgunService>();
        services.AddScoped<IFirebaseService, FirebaseService>();
        //--
        services.AddScoped<IHangfireService, HangfireService>();
        services.AddSingleton<IJobInstanceJobs, JobInstanceJobs>();
        services.AddSingleton<StudentBackgroundJobs>();
        //--
        // OrderStatusMaintenanceService + ReassignmentService registered in Application DI
        services.AddScoped<IStudentStatisticsService, StudentStatisticsService>();
        services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();


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

        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IHEmailRepository, HEmailRepository>();
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IServiceRegionRepository, ServiceRegionRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ISponsorRepository, SponsorRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<ISuspensionLogRepository, SuspensionLogRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<AdminService>();

        // Google Calendar
        services.AddSingleton<IGoogleCalendarService, GoogleCalendarService>();
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


                // 👇 Required for SignalR over WebSockets
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/hubs/notifications") || path.StartsWithSegments("/hubs/chat")))
                        {
                            // Read the token from the query string
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },

                    // Validate SecurityStamp to support token invalidation on logout/anonymization
                    OnTokenValidated = async context =>
                    {
                        var userManager = context.HttpContext.RequestServices
                            .GetRequiredService<UserManager<User>>();

                        var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var tokenSecurityStamp = context.Principal?.FindFirst("SecurityStamp")?.Value;

                        if (userId == null)
                        {
                            context.Fail("Invalid token: missing user ID");
                            return;
                        }

                        var user = await userManager.FindByIdAsync(userId);

                        if (user == null || user.SecurityStamp != tokenSecurityStamp)
                        {
                            context.Fail("Token has been invalidated");
                            return;
                        }
                    }
                };
            });

        services.AddAuthorization();


        services.AddTransient<RoleSeeder>();
        services.AddTransient<ContractNumberSequenceSeeder>();
        services.AddTransient<ServiceDataSeeder>();
        services.AddTransient<FacultyDataSeeder>();

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

            options.NotificationsArchiveFolderId = configuration["GoogleDrive:NotificationsArchiveFolderId"] ?? "";
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