using System.ComponentModel.DataAnnotations;

namespace asset_manager.Models;

public class Guide
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [StringLength(240)]
    public string? Summary { get; set; }

    public string? Content { get; set; }

    [StringLength(260)]
    public string? AttachmentName { get; set; }

    [StringLength(500)]
    public string? AttachmentPath { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [Required]
    public int GuideCategoryId { get; set; }

    public GuideCategory? GuideCategory { get; set; }
}
