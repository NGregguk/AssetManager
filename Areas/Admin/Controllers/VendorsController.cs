using asset_manager.Data;
using asset_manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace asset_manager.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class VendorsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var items = await context.Vendors.AsNoTracking().OrderBy(v => v.Name).ToListAsync();
        return View(items);
    }

    public IActionResult Create()
    {
        return View(new Vendor());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name")] Vendor vendor)
    {
        if (!ModelState.IsValid)
        {
            return View(vendor);
        }

        context.Vendors.Add(vendor);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var vendor = await context.Vendors.FindAsync(id);
        if (vendor == null)
        {
            return NotFound();
        }

        return View(vendor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Vendor vendor)
    {
        if (id != vendor.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(vendor);
        }

        context.Update(vendor);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var vendor = await context.Vendors.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
        if (vendor == null)
        {
            return NotFound();
        }

        return View(vendor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var vendor = await context.Vendors.FindAsync(id);
        if (vendor != null)
        {
            context.Vendors.Remove(vendor);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
