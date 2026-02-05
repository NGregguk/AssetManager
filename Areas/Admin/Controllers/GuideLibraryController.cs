using asset_manager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace asset_manager.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class GuideLibraryController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var categories = await context.GuideCategories.AsNoTracking()
            .Include(c => c.Guides)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return View(categories);
    }

    public async Task<IActionResult> Category(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await context.GuideCategories.AsNoTracking()
            .Include(c => c.Guides)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }
}
