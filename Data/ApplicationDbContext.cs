using asset_manager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace asset_manager.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();
    public DbSet<FloorPlan> FloorPlans => Set<FloorPlan>();
    public DbSet<AssetPin> AssetPins => Set<AssetPin>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Asset>(entity =>
        {
            entity.HasIndex(a => a.Tag);
            entity.HasIndex(a => a.SerialNumber);
        });

        builder.Entity<Assignment>(entity =>
        {
            entity.HasOne(a => a.Asset)
                .WithMany(a => a.Assignments)
                .HasForeignKey(a => a.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MaintenanceRecord>(entity =>
        {
            entity.HasOne(m => m.Asset)
                .WithMany(a => a.MaintenanceRecords)
                .HasForeignKey(m => m.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Vendor)
                .WithMany(v => v.MaintenanceRecords)
                .HasForeignKey(m => m.VendorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<FloorPlan>(entity =>
        {
            entity.HasIndex(p => p.Name);
        });

        builder.Entity<AssetPin>(entity =>
        {
            entity.HasOne(p => p.FloorPlan)
                .WithMany(fp => fp.Pins)
                .HasForeignKey(p => p.FloorPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Asset)
                .WithMany()
                .HasForeignKey(p => p.AssetId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
