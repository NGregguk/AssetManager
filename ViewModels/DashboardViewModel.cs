using asset_manager.Models;

namespace asset_manager.ViewModels;

public class DashboardViewModel
{
    public IReadOnlyList<StatCardViewModel> Stats { get; init; } = Array.Empty<StatCardViewModel>();
    public IReadOnlyList<HealthCardViewModel> HealthSummary { get; init; } = Array.Empty<HealthCardViewModel>();
    public IReadOnlyList<Asset> RecentAssets { get; init; } = Array.Empty<Asset>();
    public IReadOnlyList<MaintenanceRecord> UpcomingMaintenance { get; init; } = Array.Empty<MaintenanceRecord>();
}

public class StatCardViewModel
{
    public string Title { get; init; } = string.Empty;
    public int Value { get; init; }
    public string Helper { get; init; } = string.Empty;
}

public class HealthCardViewModel
{
    public string Title { get; init; } = string.Empty;
    public int Value { get; init; }
    public string Helper { get; init; } = string.Empty;
    public string Tone { get; init; } = "info";
}
