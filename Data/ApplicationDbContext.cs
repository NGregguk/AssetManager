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
    public DbSet<AssetAttachment> AssetAttachments => Set<AssetAttachment>();
    public DbSet<AssetActivity> AssetActivities => Set<AssetActivity>();
    public DbSet<AssetModel> AssetModels => Set<AssetModel>();
    public DbSet<GuideCategory> GuideCategories => Set<GuideCategory>();
    public DbSet<Guide> Guides => Set<Guide>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Asset>(entity =>
        {
            entity.HasIndex(a => a.Tag);
            entity.HasIndex(a => a.SerialNumber);

            entity.HasOne(a => a.AssetModel)
                .WithMany(m => m.Assets)
                .HasForeignKey(a => a.AssetModelId)
                .OnDelete(DeleteBehavior.SetNull);
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

        builder.Entity<AssetAttachment>(entity =>
        {
            entity.HasIndex(a => a.AssetId);
            entity.HasOne(a => a.Asset)
                .WithMany(a => a.Attachments)
                .HasForeignKey(a => a.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AssetActivity>(entity =>
        {
            entity.HasIndex(a => a.AssetId);
            entity.HasIndex(a => a.CreatedAt);
            entity.HasOne(a => a.Asset)
                .WithMany(a => a.Activities)
                .HasForeignKey(a => a.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AssetModel>(entity =>
        {
            entity.HasIndex(m => m.Name);
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
                .WithMany(a => a.Pins)
                .HasForeignKey(p => p.AssetId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<GuideCategory>(entity =>
        {
            entity.HasIndex(c => c.Name);
        });

        builder.Entity<Guide>(entity =>
        {
            entity.HasIndex(g => g.Title);
            entity.HasIndex(g => g.GuideCategoryId);

            entity.HasOne(g => g.GuideCategory)
                .WithMany(c => c.Guides)
                .HasForeignKey(g => g.GuideCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
