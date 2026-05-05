using Microsoft.EntityFrameworkCore;
using WeatherMicroservice.Models;

namespace WeatherMicroservice.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<WeatherSnapshot> WeatherSnapshots => Set<WeatherSnapshot>();
    public DbSet<AlertSubscription> AlertSubscriptions => Set<AlertSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherSnapshot>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.Location, x.RecordedAt });
            e.Property(x => x.Location).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<AlertSubscription>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.Email, x.Location });
            e.Property(x => x.Email).HasMaxLength(320).IsRequired();
            e.Property(x => x.Location).HasMaxLength(200).IsRequired();
        });
    }
}