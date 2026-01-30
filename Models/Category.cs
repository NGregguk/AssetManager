using System.ComponentModel.DataAnnotations;

namespace asset_manager.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(400)]
    public string? Description { get; set; }

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
