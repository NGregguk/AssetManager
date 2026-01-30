using System.ComponentModel.DataAnnotations;

namespace asset_manager.Models;

public class Assignment
{
    public int Id { get; set; }

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    [Required]
    [StringLength(140)]
    public string AssignedTo { get; set; } = string.Empty;

    public DateOnly AssignedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateOnly? ReturnedDate { get; set; }

    [StringLength(400)]
    public string? Notes { get; set; }
}
