using System.ComponentModel.DataAnnotations;

namespace asset_manager.Models;

public class GuideCategory
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(240)]
    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public ICollection<Guide> Guides { get; set; } = new List<Guide>();
}
