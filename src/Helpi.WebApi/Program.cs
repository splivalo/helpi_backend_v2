
using FluentValidation;
using FluentValidation.AspNetCore;
using Helpi.Application;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Validators;
using Helpi.Infrastructure.BackgroundJobs.Jobs;
using Helpi.Infrastructure.Configuration;
using Helpi.Infrastructure.Seeds;
using Helpi.WebApi.Hubs;
using Helpi.WebApi.Middleware;
using Helpi.WebAPI.Services;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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



FirebaseConfiguration.InitializeFirebase(builder.Configuration);



builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<OrderCreateDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PricingConfigurationDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ContactInfoCreateDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AdminRegisterDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<StudentRegisterDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CustomerRegisterDtoValidator>();

builder.Services.AddCors(options =>
{



    options.AddPolicy("AllowFlutterWeb",
 policy => policy
     .WithOrigins("http://localhost:59013")
     .AllowAnyMethod()
     .AllowAnyHeader()
    );

    /// todo : remove in prod
    options.AddPolicy("AllowAll", policy => policy
     .AllowAnyOrigin()  // For development only! Replace with your Flutter web URL in production.
     .AllowAnyMethod()
     .AllowAnyHeader());
});


builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>() // 👈 Loads secrets for dev
    .AddEnvironmentVariables(); // 👈 Prepares for prod



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard();

await app.Services.SeedAsync(); // important

/// TODO: seeders
using (var scope = app.Services.CreateScope())
{
    var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
    await roleSeeder.SeedRolesAsync();

    var contractNumberSequenceSeeder = scope.ServiceProvider.GetRequiredService<ContractNumberSequenceSeeder>();
    await contractNumberSequenceSeeder.SeedAsync();

    //
    // var citySeeder = scope.ServiceProvider.GetRequiredService<CitySeeder>();
    // await citySeeder.SeedAsync();


    var serviceDataSeeder = scope.ServiceProvider.GetRequiredService<ServiceDataSeeder>();
    await serviceDataSeeder.SeedAsync();

}

using (var scope = app.Services.CreateScope())
{
    var jobInstanceJobs = scope.ServiceProvider.GetRequiredService<IJobInstanceJobs>();
    jobInstanceJobs.GenerateFutureJobInstances();
    jobInstanceJobs.ScheduleDailyStatusUpdates();
    jobInstanceJobs.ScheduleDailyJobInstancePayments();


    var studentBackgroundJobs = scope.ServiceProvider.GetRequiredService<StudentBackgroundJobs>();
    studentBackgroundJobs.ProcessStudentContracts();
}




app.UseCors("AllowFlutterLocalhost");
app.UseCors("AllowAll");
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

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notification");

app.Run();

