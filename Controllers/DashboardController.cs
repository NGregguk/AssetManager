using asset_manager.Data;
using asset_manager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace asset_manager.Controllers;

[Authorize]
public class DashboardController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var soon = today.AddDays(30);
        var warrantyWindow = today.AddDays(90);

        var totalAssets = await context.Assets.CountAsync();
        var assignedAssets = await context.Assignments.CountAsync(a => a.ReturnedDate == null);
        var maintenanceDueSoon = await context.MaintenanceRecords
            .CountAsync(m => m.NextDueDate != null && m.NextDueDate <= soon);
        var vendorCount = await context.Vendors.CountAsync();
        var overdueMaintenance = await context.MaintenanceRecords
            .CountAsync(m => m.NextDueDate != null && m.NextDueDate < today);
        var warrantyExpiring = await context.Assets
            .CountAsync(a => a.WarrantyExpiry != null && a.WarrantyExpiry >= today && a.WarrantyExpiry <= warrantyWindow);
        var unassignedAssets = await context.Assets
            .CountAsync(a => !context.Assignments.Any(x => x.AssetId == a.Id && x.ReturnedDate == null));

        var recentAssets = await context.Assets
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Location)
            .OrderByDescending(a => a.Id)
            .Take(6)
            .ToListAsync();

        var upcomingMaintenance = await context.MaintenanceRecords
            .AsNoTracking()
            .Include(m => m.Asset)
            .Include(m => m.Vendor)
            .Where(m => m.NextDueDate != null && m.NextDueDate >= today)
            .OrderBy(m => m.NextDueDate)
            .Take(6)
            .ToListAsync();

        var viewModel = new DashboardViewModel
        {
            Stats =
            [
                new StatCardViewModel { Title = "Total assets", Value = totalAssets, Helper = "All tracked assets" },
                new StatCardViewModel { Title = "Assigned", Value = assignedAssets, Helper = "Currently with people" },
                new StatCardViewModel { Title = "Maintenance due", Value = maintenanceDueSoon, Helper = "Next 30 days" },
                new StatCardViewModel { Title = "Vendors", Value = vendorCount, Helper = "Active suppliers" }
            ],
            HealthSummary =
            [
                new HealthCardViewModel { Title = "Overdue maintenance", Value = overdueMaintenance, Helper = "Past due tasks", Tone = "danger" },
                new HealthCardViewModel { Title = "Warranty expiring", Value = warrantyExpiring, Helper = "Next 90 days", Tone = "warning" },
                new HealthCardViewModel { Title = "Unassigned assets", Value = unassignedAssets, Helper = "Not with people", Tone = "info" }
            ],
            RecentAssets = recentAssets,
            UpcomingMaintenance = upcomingMaintenance
        };

        return View(viewModel);
    }
}
