
using FluentValidation;
using FluentValidation.AspNetCore;
using Helpi.Application;
using Helpi.Application.Interfaces.Services;
using Helpi.Infrastructure.Configuration;
using Helpi.Infrastructure.Seeds;
using Helpi.WebApi.Middleware;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddGoogleDriveServices(builder.Configuration);


builder.Services.AddApplication(builder.Configuration);


builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddHangfireServices(builder.Configuration);


FirebaseConfiguration.InitializeFirebase(builder.Configuration);



builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<OrderCreateDtoValidator>();

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



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard();

/// TODO: seeders
using (var scope = app.Services.CreateScope())
{
    var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
    await roleSeeder.SeedRolesAsync();

    var contractNumberSequenceSeeder = scope.ServiceProvider.GetRequiredService<ContractNumberSequenceSeeder>();
    await contractNumberSequenceSeeder.SeedAsync();

    //
    var citySeeder = scope.ServiceProvider.GetRequiredService<CitySeeder>();
    await citySeeder.SeedAsync();


    var serviceDataSeeder = scope.ServiceProvider.GetRequiredService<ServiceDataSeeder>();
    await serviceDataSeeder.SeedAsync();

}

using (var scope = app.Services.CreateScope())
{
    var jobInstanceJobs = scope.ServiceProvider.GetRequiredService<IJobInstanceJobs>();
    jobInstanceJobs.GenerateFutureJobInstances();
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

app.Run();

