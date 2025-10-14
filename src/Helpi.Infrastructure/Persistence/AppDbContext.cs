
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence.Extentions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using NetTopologySuite;
using Npgsql;

namespace Helpi.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {

    }


    // public AppDbContext() { }

    // Core Identity & Authentication
    // public DbSet<User> Users { get; set; }
    // public DbSet<RefreshToken> RefreshTokens { get; set; }

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
    // public DbSet<ScheduleAssignmentReplacement> ScheduleAssignmentReplacements { get; set; }

    // Feedback & Billing
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<HEmail> InvoiceEmails { get; set; }

    // Geographic Data
    public DbSet<City> Cities { get; set; }
    public DbSet<ServiceRegion> ServiceRegions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        // Connection string

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();



        var connectionString = configuration.GetConnectionString("Default");


        /// todo: remove 
        // using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        // var logger = loggerFactory.CreateLogger("DbContext");
        // logger.LogInformation($"🔥 DB connection -> {connectionString}");




        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        // dataSourceBuilder.UseNetTopologySuite();

        var dataSource = dataSourceBuilder.Build();

        optionsBuilder.UseNpgsql(
            dataSource,
            o => o.UseNetTopologySuite()
        );



    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {





        // // Configure entity mappings here
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        // base.OnModelCreating(modelBuilder);

        base.OnModelCreating(modelBuilder);

        // Automatically make all int PKs value-generated
        // foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        // {
        //     var key = entityType.FindPrimaryKey();
        //     if (key != null)
        //     {
        //         foreach (var property in key.Properties)
        //         {
        //             if (property.ClrType == typeof(int))
        //             {
        //                 property.ValueGenerated = ValueGenerated.OnAdd;
        //             }
        //         }
        //     }
        // }
        /// ====



        //     modelBuilder.Entity<ContactInfo>()
        //    .Property(c => c.DateOfBirth)
        //    .HasConversion(
        //        v => v.ToUniversalTime(), // Convert to UTC before saving
        //        v => DateTime.SpecifyKind(v, DateTimeKind.Utc) // Convert to UTC when reading
        //    );



        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Spatial configuration for PostgreSQL
        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.Property(c => c.Translations)
                       .HasColumnType("jsonb");
        });




        modelBuilder.Entity<Service>(entity =>
          {
              // Configure JSON conversion for Translations
              entity.Property(e => e.Translations)
                    .HasColumnType("jsonb");
          });

        // Configure composite primary key
        modelBuilder.Entity<OrderService>().HasKey(os => new
        {
            os.OrderId,
            os.ServiceId
        });

        modelBuilder.Entity<Order>(entity =>
           {
               entity.Property(o => o.Notes)
                .HasMaxLength(1000)
                .IsUnicode(true);
           });




        // fcm token
        modelBuilder.Entity<FcmToken>()
     .HasKey(t => new { t.UserId, t.Token });




        modelBuilder.Entity<StudentAvailabilitySlot>()
        .HasKey(s => new { s.StudentId, s.DayOfWeek });

        modelBuilder.Entity<StudentAvailabilitySlot>()
            .HasOne(s => s.Student)
            .WithMany(st => st.AvailabilitySlots)
            .HasForeignKey(s => s.StudentId);


        modelBuilder.Entity<StudentService>()
            .HasKey(ss => new { ss.StudentId, ss.ServiceId });

        // modelBuilder.Entity<ScheduleAssignmentReplacement>()
        //     .HasOne(sar => sar.OriginalAssignment)
        //     .WithMany()
        //     .OnDelete(DeleteBehavior.Restrict);

        // modelBuilder.Entity<ScheduleAssignmentReplacement>()
        //     .HasOne(sar => sar.NewAssignment)
        //     .WithMany()
        //     .OnDelete(DeleteBehavior.Restrict);

        // Configure spatial index for Cities
        modelBuilder.Entity<City>(entity =>
           {
               entity.HasIndex(c => c.GooglePlaceId).IsUnique();
               //    entity.HasIndex(c => c.Bounds).HasMethod("GIST");
               //    entity.Property(e => e.Bounds).HasColumnType("geometry (Polygon)");
               //    entity.Property(c => c.Bounds).IsRequired(false);
           });

        // Configure JSON column for Senior special requirements
        modelBuilder.Entity<Senior>()
            .Property(s => s.SpecialRequirements)
            .HasColumnType("jsonb");

        modelBuilder.Entity<ServiceRegion>(entity =>
            {
                entity.HasIndex(sr => new { sr.CityId, sr.ServiceId }).IsUnique();
            });

        modelBuilder.Entity<JobInstance>(entity =>
        {
            entity.HasOne(j => j.ScheduleAssignment)          // JobInstance has one ScheduleAssignment
              .WithMany(s => s.JobInstances)      // ScheduleAssignment has many JobInstances
              .HasForeignKey(j => j.ScheduleAssignmentId); // Foreign key

            entity.Property(p => p.HourlyRate).HasColumnType("decimal(18,2)");
            entity.Property(p => p.CompanyPercentage).HasColumnType("decimal(5,2)");
            entity.Property(p => p.ServiceProviderPercentage).HasColumnType("decimal(5,2)");

            entity.Property(j => j.Notes)
            .HasMaxLength(1000)
            .IsUnicode(true);

            entity.HasOne(j => j.PaymentTransaction)
                    .WithOne(p => p.JobInstance)
                    .HasForeignKey<PaymentTransaction>(p => p.JobInstanceId)
                    .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PricingConfiguration>(entity =>
        {
            entity.Property(p => p.JobHourlyRate).HasColumnType("decimal(18,2)");
            entity.Property(p => p.CompanyPercentage).HasColumnType("decimal(5,2)");
            entity.Property(p => p.ServiceProviderPercentage).HasColumnType("decimal(5,2)");
        });




        // modelBuilder.Entity<User>()
        //           .HasOne(u => u.Customer)
        //           .WithOne(c => c.User)
        //           .HasForeignKey<Customer>(c => c.Id)
        //           .OnDelete(DeleteBehavior.Cascade);

        // modelBuilder.Entity<User>()
        // .HasOne(s => s.Student)
        // .WithOne(s => s.User)
        // .HasForeignKey<Student>(s => s.Id)
        // .OnDelete(DeleteBehavior.Cascade);

        /// Indexing
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