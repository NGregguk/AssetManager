using System.ComponentModel.DataAnnotations;

namespace asset_manager.Models;

public class AssetPin
{
    public int Id { get; set; }

    public int FloorPlanId { get; set; }
    public FloorPlan FloorPlan { get; set; } = null!;

    public int? AssetId { get; set; }
    public Asset? Asset { get; set; }

    [Range(0, 100)]
    public decimal XPercent { get; set; }

    [Range(0, 100)]
    public decimal YPercent { get; set; }

    [StringLength(160)]
    public string? Label { get; set; }

    [StringLength(400)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
