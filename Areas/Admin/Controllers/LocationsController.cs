using asset_manager.Data;
using asset_manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace asset_manager.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class LocationsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var items = await context.Locations.AsNoTracking().OrderBy(l => l.Name).ToListAsync();
        return View(items);
    }

    public IActionResult Create()
    {
        return View(new Location());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name")] Location location)
    {
        if (!ModelState.IsValid)
        {
            return View(location);
        }

        context.Locations.Add(location);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var location = await context.Locations.FindAsync(id);
        if (location == null)
        {
            return NotFound();
        }

        return View(location);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Location location)
    {
        if (id != location.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(location);
        }

        context.Update(location);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var location = await context.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        if (location == null)
        {
            return NotFound();
        }

        return View(location);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var location = await context.Locations.FindAsync(id);
        if (location != null)
        {
            context.Locations.Remove(location);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
