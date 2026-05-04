
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence.Extentions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Helpi.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<HNotification> HNotifications { get; set; }
    public DbSet<PricingConfiguration> PricingConfigurations { get; set; }
    public DbSet<PricingChangeHistory> PricingChangeHistories { get; set; }

    public DbSet<PaymentProfile> PaymentProfiles { get; set; }
    public DbSet<ContractNumberSequence> ContractNumberSequences { get; set; }

    // Contact Information

    public DbSet<ContactInfo> ContactInfos { get; set; }
    public DbSet<FcmToken> FcmTokens { get; set; }
    public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }

    // Academic Structure
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Faculty> Faculties { get; set; }
    public DbSet<StudentContract> StudentContracts { get; set; }

    // Customer Relationships
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Senior> Seniors { get; set; }

    // Payment System
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    // Service Catalog
    public DbSet<ServiceCategory> ServiceCategories { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<StudentService> StudentServices { get; set; }

    // Availability & Scheduling
    public DbSet<StudentAvailabilitySlot> StudentAvailabilitySlots { get; set; }
    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderService> OrderServices { get; set; }
    public DbSet<OrderSchedule> OrderSchedules { get; set; }

    // Job Management
    public DbSet<JobRequest> JobRequests { get; set; }
    public DbSet<ScheduleAssignment> ScheduleAssignments { get; set; }
    public DbSet<ReassignmentRecord> ReassignmentRecords { get; set; }
    public DbSet<JobInstance> JobInstances { get; set; }

    // Feedback & Billing
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<HEmail> InvoiceEmails { get; set; }

    // Geographic Data
    public DbSet<City> Cities { get; set; }
    public DbSet<ServiceRegion> ServiceRegions { get; set; }

    // Suspension
    public DbSet<SuspensionLog> SuspensionLogs { get; set; }

    // Admin Notes
    public DbSet<AdminNote> AdminNotes { get; set; }

    // Chat
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    // Coupons
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<CouponAssignment> CouponAssignments { get; set; }
    public DbSet<CouponUsage> CouponUsages { get; set; }

    // Sponsors
    public DbSet<Sponsor> Sponsors { get; set; }

    // Google Calendar
    public DbSet<GoogleCalendarToken> GoogleCalendarTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();

        var dataSource = dataSourceBuilder.Build();

        optionsBuilder.UseNpgsql(
            dataSource,
            o => o.UseNetTopologySuite()
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.AddCustomIndexes();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateOnly>()
            .HaveConversion<DateOnlyConverter>()
            .HaveColumnType("date");

        configurationBuilder.Properties<TimeOnly>()
            .HaveConversion<TimeOnlyConverter>()
            .HaveColumnType("time");

        // Ensure all DateTime are stored as UTC in PostgreSQL
        configurationBuilder.Properties<DateTime>()
          .HaveConversion<UtcDateTimeConverter>()
          .HaveColumnType("timestamp with time zone");

        configurationBuilder.Properties<DateTime?>()
            .HaveConversion<UtcNullableDateTimeConverter>()
            .HaveColumnType("timestamp with time zone");
    }

}



// DateOnly/TimeOnly converters for EF Core 8+
public class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyConverter() : base(
        d => d.ToDateTime(TimeOnly.MinValue),
        d => DateOnly.FromDateTime(d))
    { }
}

public class TimeOnlyConverter : ValueConverter<TimeOnly, TimeSpan>
{
    public TimeOnlyConverter() : base(
        t => t.ToTimeSpan(),
        t => TimeOnly.FromTimeSpan(t))
    { }
}

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    { }
}

public class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public UtcNullableDateTimeConverter()
        : base(
            v => v.HasValue
                ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : v.Value.ToUniversalTime())
                : v,
            v => v.HasValue
                ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
                : v)
    { }
}