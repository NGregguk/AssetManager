using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace asset_manager.ViewModels;

public class GuideEditorViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [StringLength(240)]
    public string? Summary { get; set; }

    public string? Content { get; set; }

    public IFormFile? Attachment { get; set; }

    public string? ExistingAttachmentName { get; set; }

    public string? ExistingAttachmentPath { get; set; }

    public bool RemoveAttachment { get; set; }

    [Required]
    public int GuideCategoryId { get; set; }

    public int SortOrder { get; set; }
}
