using System.ComponentModel.DataAnnotations;

namespace asset_manager.Models;

public class AssetAttachment
{
    public int Id { get; set; }

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    [Required]
    [StringLength(260)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [StringLength(260)]
    public string OriginalFileName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? ContentType { get; set; }

    public long SizeBytes { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
