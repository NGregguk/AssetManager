using System.ComponentModel.DataAnnotations;

namespace asset_manager.ViewModels;

public class AssetCheckoutViewModel
{
    public int AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? CurrentAssignee { get; set; }
    public DateOnly? CurrentExpectedReturnDate { get; set; }

    [StringLength(140)]
    public string? AssignedTo { get; set; }

    public DateOnly? ExpectedReturnDate { get; set; }

    public DateOnly? ReturnedDate { get; set; }
}
