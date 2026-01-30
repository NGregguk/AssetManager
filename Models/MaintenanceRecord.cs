using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace asset_manager.Models;

public class MaintenanceRecord
{
    public int Id { get; set; }

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public DateOnly MaintenanceDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateOnly? NextDueDate { get; set; }

    [StringLength(200)]
    public string? PerformedBy { get; set; }

    [StringLength(600)]
    public string Summary { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Cost { get; set; }
}
