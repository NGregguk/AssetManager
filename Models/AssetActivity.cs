using System.ComponentModel.DataAnnotations;

namespace asset_manager.Models;

public class AssetActivity
{
    public int Id { get; set; }

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public AssetActivityType ActivityType { get; set; }

    [Required]
    [StringLength(400)]
    public string Message { get; set; } = string.Empty;

    [StringLength(160)]
    public string? PerformedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
