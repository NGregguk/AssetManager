using asset_manager.Data;
using asset_manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace asset_manager.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AssetModelsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var items = await context.AssetModels.AsNoTracking().OrderBy(m => m.Name).ToListAsync();
        return View(items);
    }

    public IActionResult Create()
    {
        return View(new AssetModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Manufacturer,ModelNumber,Architecture,Cpu,OperatingSystem,OsVersion,OsBuild,TotalRamGb,Notes")] AssetModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        context.AssetModels.Add(model);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var model = await context.AssetModels.FindAsync(id);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Manufacturer,ModelNumber,Architecture,Cpu,OperatingSystem,OsVersion,OsBuild,TotalRamGb,Notes")] AssetModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        context.Update(model);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var model = await context.AssetModels.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var model = await context.AssetModels.FindAsync(id);
        if (model != null)
        {
            context.AssetModels.Remove(model);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
