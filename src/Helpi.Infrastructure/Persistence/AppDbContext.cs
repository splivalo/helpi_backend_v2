
using Helpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(
            "YourConnectionString",
            o => o.UseNetTopologySuite()
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<City>(entity =>
{
    entity.HasIndex(c => c.GooglePlaceId).IsUnique();
    // entity.HasIndex(c => c.Bounds).IsSpatial(); // TODO:
});

        modelBuilder.Entity<ServiceRegion>(entity =>
        {
            entity.HasIndex(sr => new { sr.CityId, sr.ServiceId }).IsUnique();
        });

        // Configure entity mappings here
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }


}