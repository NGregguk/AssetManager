using System.ComponentModel.DataAnnotations;

namespace asset_manager.Models;

public class FloorPlan
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(260)]
    public string ImagePath { get; set; } = string.Empty;

    public ICollection<AssetPin> Pins { get; set; } = new List<AssetPin>();
}
