using FluentValidation;
using FluentValidation.AspNetCore;
using Helpi.Application;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Services;
using Helpi.Application.Validators;
using Helpi.Domain.Events;
using Helpi.Infrastructure.BackgroundJobs.Jobs;
using Helpi.Infrastructure.Configuration;
using Helpi.Infrastructure.Seeds;
using Helpi.WebApi.Hubs;
using Helpi.WebApi.Middleware;
using Helpi.WebAPI.Services;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// ------------------------------------------------------------
// 1️⃣  CONFIGURATION & LOGGING
// ------------------------------------------------------------
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()      // Development secrets
    .AddEnvironmentVariables();     // Environment overrides

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ------------------------------------------------------------
// 2️⃣  SERVICE REGISTRATION
// ------------------------------------------------------------

// Core framework services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure & application layers
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddHangfireServices(builder.Configuration);
builder.Services.AddSignalRServices(builder.Configuration);
builder.Services.AddGoogleDriveServices(builder.Configuration);

// SignalR notifications
builder.Services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();

// FluentValidation (automatic model validation)
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<OrderCreateDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<OrderScheduleCreateDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PricingConfigurationDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ContactInfoCreateDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AdminRegisterDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<StudentRegisterDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CustomerRegisterDtoValidator>();

// CORS policies
builder.Services.AddCors(options =>
{
    if (env.IsDevelopment())
    {
        options.AddPolicy("AllowAll", policy => policy
            .AllowAnyOrigin()   // Development only
            .AllowAnyMethod()
            .AllowAnyHeader());
    }
    else
    {
        options.AddPolicy("AllowFlutterAdminDashboard", policy => policy
            .WithOrigins("https://admin.helpi.social")
            .AllowAnyMethod()
            .AllowAnyHeader());
    }
});





// ------------------------------------------------------------
// 3️⃣  BUILD APP
// ------------------------------------------------------------
var app = builder.Build();



// Initialize Firebase
var logger = app.Services.GetRequiredService<ILogger<Program>>();
FirebaseConfiguration.InitializeFirebase(builder.Configuration, logger);

// ------------------------------------------------------------
// 4️⃣  MIDDLEWARE PIPELINE
// ------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



// Pre-flight CORS handling (must be early)
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
app.UseCors("AllowFlutterAdminDashboard");
app.UseCors("AllowAllForPreflight");

// Serve static files
app.UseStaticFiles();
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Short-circuit OPTIONS requests
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 204;
        await context.Response.CompleteAsync();
        return;
    }
    await next();
});

app.UseRouting();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// app.UseHttpsRedirection(); // Disabled on server—enable when HTTPS is ready

app.UseAuthentication();
app.UseAuthorization();

// Hangfire dashboard (background jobs)
app.UseHangfireDashboard();

// ------------------------------------------------------------
// 5️⃣  ENDPOINTS
// ------------------------------------------------------------
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications").RequireAuthorization();


app.UseWebSockets();

// ------------------------------------------------------------
// 6️⃣  STARTUP TASKS (Seed data, subscribe events, schedule jobs)
// ------------------------------------------------------------

// Domain event subscriptions
using (var scope = app.Services.CreateScope())
{
    var mediator = scope.ServiceProvider.GetRequiredService<IEventMediator>();
    mediator?.Subscribe<ReinitiateAllFailedMatchesEvent, FailedMatchReinitiationService>();
}

// Seed essential data
using (var scope = app.Services.CreateScope())
{
    var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
    await roleSeeder.SeedRolesAsync();

    var contractSeeder = scope.ServiceProvider.GetRequiredService<ContractNumberSequenceSeeder>();
    await contractSeeder.SeedAsync();

    var serviceDataSeeder = scope.ServiceProvider.GetRequiredService<ServiceDataSeeder>();
    await serviceDataSeeder.SeedAsync();

    var facultyDataSeeder = scope.ServiceProvider.GetRequiredService<FacultyDataSeeder>();
    await facultyDataSeeder.SeedAsync();

    await app.Services.SeedPriceConfigAsync();
}

// Hangfire recurring jobs
using (var scope = app.Services.CreateScope())
{
    var studentJobs = scope.ServiceProvider.GetRequiredService<StudentBackgroundJobs>();
    studentJobs.ProcessStudentContracts();

    var jobInstanceJobs = scope.ServiceProvider.GetRequiredService<IJobInstanceJobs>();
    jobInstanceJobs.GenerateFutureJobInstances();
    jobInstanceJobs.ScheduleDailyStatusUpdates();
    jobInstanceJobs.ScheduleDailyJobInstancePayments();
}

// ------------------------------------------------------------
// 7️⃣  RUN APPLICATION
// ------------------------------------------------------------
app.Run();
