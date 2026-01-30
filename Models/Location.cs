using System.ComponentModel.DataAnnotations;

namespace asset_manager.Models;

public class Location
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Building { get; set; }

    [StringLength(200)]
    public string? Room { get; set; }

    [StringLength(400)]
    public string? Notes { get; set; }

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
