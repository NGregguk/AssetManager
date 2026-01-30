using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace asset_manager.Models;

public class Asset
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Tag { get; set; }

    [StringLength(120)]
    public string? SerialNumber { get; set; }

    [StringLength(120)]
    public string? ComputerName { get; set; }

    [StringLength(80)]
    public string? Architecture { get; set; }

    [StringLength(120)]
    public string? BiosSerial { get; set; }

    [StringLength(120)]
    public string? BiosVersion { get; set; }

    public int? Cores { get; set; }

    [StringLength(160)]
    public string? Cpu { get; set; }

    public DateOnly? InstallDate { get; set; }

    public int? LogicalProcessors { get; set; }

    [StringLength(120)]
    public string? Manufacturer { get; set; }

    [StringLength(120)]
    public string? Model { get; set; }

    [StringLength(160)]
    public string? OperatingSystem { get; set; }

    [StringLength(80)]
    public string? OsBuild { get; set; }

    [StringLength(80)]
    public string? OsVersion { get; set; }

    [StringLength(120)]
    public string? Space { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? TotalRamGb { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PurchaseCost { get; set; }

    public DateOnly? WarrantyExpiry { get; set; }

    [StringLength(600)]
    public string? Description { get; set; }

    public AssetStatus Status { get; set; } = AssetStatus.InStock;

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? LocationId { get; set; }
    public Location? Location { get; set; }

    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
}
