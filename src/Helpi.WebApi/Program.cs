
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

builder.Services
    .AddInfrastructure(builder.Configuration);


builder.Services.AddApplication();


builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddHangfireServices(builder.Configuration);


FirebaseConfiguration.InitializeFirebase(builder.Configuration);

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





app.UseRouting();



app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();

