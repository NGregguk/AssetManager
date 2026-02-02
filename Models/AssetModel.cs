using System.ComponentModel.DataAnnotations;

namespace asset_manager.Models;

public class AssetModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [StringLength(160)]
    public string? Manufacturer { get; set; }

    [StringLength(160)]
    public string? ModelNumber { get; set; }

    [StringLength(80)]
    public string? Architecture { get; set; }

    [StringLength(160)]
    public string? Cpu { get; set; }

    [StringLength(160)]
    public string? OperatingSystem { get; set; }

    [StringLength(80)]
    public string? OsBuild { get; set; }

    [StringLength(80)]
    public string? OsVersion { get; set; }

    public decimal? TotalRamGb { get; set; }

    [StringLength(400)]
    public string? Notes { get; set; }

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
