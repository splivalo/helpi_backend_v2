
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
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddGoogleDriveServices(builder.Configuration);

// here because of NotificationHub
builder.Services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();

builder.Services.AddSignalRServices(builder.Configuration);


builder.Services.AddApplication(builder.Configuration);


builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddHangfireServices(builder.Configuration);




builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<OrderCreateDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<OrderScheduleCreateDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PricingConfigurationDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ContactInfoCreateDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AdminRegisterDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<StudentRegisterDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CustomerRegisterDtoValidator>();

builder.Services.AddCors(options =>
{
    if (env.IsDevelopment())
    {
        options.AddPolicy("AllowAll", policy => policy
           .AllowAnyOrigin()  // For development ONLY! 
           .AllowAnyMethod()
           .AllowAnyHeader());
    }
    else
    {
        options.AddPolicy("AllowFlutterAdminDashboard", policy => policy
               .WithOrigins("https://admin.helpi.social")
               .AllowAnyMethod()
               .AllowAnyHeader()
               );
    }
});


builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>() // 👈 Loads secrets for dev
    .AddEnvironmentVariables(); // 👈 Prepares for prod

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
FirebaseConfiguration.InitializeFirebase(builder.Configuration, logger);



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard();


using (var scope = app.Services.CreateScope())
{
    var mediator = scope.ServiceProvider.GetRequiredService<IEventMediator>();
    mediator?.Subscribe<ReinitiateAllFailedMatchesEvent, FailedMatchReinitiationService>();
}



/// Seeds
using (var scope = app.Services.CreateScope())
{
    var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
    await roleSeeder.SeedRolesAsync();

    var contractNumberSequenceSeeder = scope.ServiceProvider.GetRequiredService<ContractNumberSequenceSeeder>();
    await contractNumberSequenceSeeder.SeedAsync();

    var serviceDataSeeder = scope.ServiceProvider.GetRequiredService<ServiceDataSeeder>();
    await serviceDataSeeder.SeedAsync();

    await app.Services.SeedPriceConfigAsync();
}


/// Hangfire schedules
using (var scope = app.Services.CreateScope())
{
    var jobInstanceJobs = scope.ServiceProvider.GetRequiredService<IJobInstanceJobs>();
    jobInstanceJobs.GenerateFutureJobInstances();
    jobInstanceJobs.ScheduleDailyStatusUpdates();
    jobInstanceJobs.ScheduleDailyJobInstancePayments();

    //
    var studentBackgroundJobs = scope.ServiceProvider.GetRequiredService<StudentBackgroundJobs>();
    studentBackgroundJobs.ProcessStudentContracts();
}


if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}

app.UseCors("AllowFlutterAdminDashboard");
app.UseCors("AllowAllForPreflight"); // Apply CORS middleware early in the pipeline

app.Use(async (context, next) =>
{

    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 204; // No Content
        await context.Response.CompleteAsync();
        return; // Short-circuit the pipeline for OPTIONS
    }
    await next();
});



app.UseRouting();



app.UseMiddleware<ExceptionHandlingMiddleware>();

// app.UseHttpsRedirection(); <-- problem on server


app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notification");

app.Run();

