using asset_manager.Models;

namespace asset_manager.ViewModels;

public class FloorPlanMapViewModel
{
    public IReadOnlyList<FloorPlan> Plans { get; init; } = Array.Empty<FloorPlan>();
    public FloorPlan? SelectedPlan { get; init; }
    public IReadOnlyList<AssetPin> Pins { get; init; } = Array.Empty<AssetPin>();
    public IReadOnlyList<Asset> Assets { get; init; } = Array.Empty<Asset>();
    public int? FilterAssetId { get; init; }
}
