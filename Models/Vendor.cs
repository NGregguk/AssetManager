using System.ComponentModel.DataAnnotations;

namespace asset_manager.Models;

public class Vendor
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [StringLength(120)]
    public string? ContactName { get; set; }

    [StringLength(140)]
    public string? Email { get; set; }

    [StringLength(40)]
    public string? Phone { get; set; }

    [StringLength(400)]
    public string? Notes { get; set; }

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
}
