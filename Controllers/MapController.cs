using asset_manager.Data;
using asset_manager.Models;
using asset_manager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace asset_manager.Controllers;

[Authorize]
public class MapController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(int? planId, int? assetId)
    {
        var plans = await context.FloorPlans.AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();

        if (plans.Count == 0)
        {
            return View(new FloorPlanMapViewModel { Plans = plans });
        }

        FloorPlan? selectedPlan = null;

        if (assetId.HasValue)
        {
            var pinPlanId = await context.AssetPins.AsNoTracking()
                .Where(p => p.AssetId == assetId.Value)
                .Select(p => p.FloorPlanId)
                .FirstOrDefaultAsync();

            if (pinPlanId != 0)
            {
                selectedPlan = plans.FirstOrDefault(p => p.Id == pinPlanId);
            }
        }

        selectedPlan ??= planId.HasValue
            ? plans.FirstOrDefault(p => p.Id == planId.Value)
            : plans.First();

        if (selectedPlan == null)
        {
            selectedPlan = plans.First();
        }

        var pinsQuery = context.AssetPins.AsNoTracking()
            .Include(p => p.Asset)
                .ThenInclude(a => a!.Category)
            .Include(p => p.Asset)
                .ThenInclude(a => a!.Location)
            .Where(p => p.FloorPlanId == selectedPlan.Id);

        if (assetId.HasValue)
        {
            pinsQuery = pinsQuery.Where(p => p.AssetId == assetId.Value);
        }

        var pins = await pinsQuery
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var assets = await context.Assets.AsNoTracking()
            .OrderBy(a => a.Name)
            .ToListAsync();

        return View(new FloorPlanMapViewModel
        {
            Plans = plans,
            SelectedPlan = selectedPlan,
            Pins = pins,
            Assets = assets,
            FilterAssetId = assetId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPin(int planId, int? assetId, decimal xPercent, decimal yPercent, string? label, string? notes)
    {
        if (xPercent < 0 || xPercent > 100 || yPercent < 0 || yPercent > 100)
        {
            return RedirectToAction(nameof(Index), new { planId });
        }

        var planExists = await context.FloorPlans.AnyAsync(p => p.Id == planId);
        if (!planExists)
        {
            return RedirectToAction(nameof(Index));
        }

        var pin = new AssetPin
        {
            FloorPlanId = planId,
            AssetId = assetId,
            XPercent = xPercent,
            YPercent = yPercent,
            Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };

        context.AssetPins.Add(pin);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { planId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePin(int id, int planId)
    {
        var pin = await context.AssetPins.FindAsync(id);
        if (pin != null)
        {
            context.AssetPins.Remove(pin);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index), new { planId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePin(int id, decimal xPercent, decimal yPercent)
    {
        var pin = await context.AssetPins.FindAsync(id);
        if (pin == null)
        {
            return NotFound();
        }

        if (xPercent < 0 || xPercent > 100 || yPercent < 0 || yPercent > 100)
        {
            return BadRequest();
        }

        pin.XPercent = xPercent;
        pin.YPercent = yPercent;
        await context.SaveChangesAsync();

        return Ok();
    }
}
