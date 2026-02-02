using asset_manager.Data;
using asset_manager.Models;
using asset_manager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace asset_manager.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class FloorPlansController(ApplicationDbContext context, IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index()
    {
        var plans = await context.FloorPlans.AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
        return View(plans);
    }

    public IActionResult Create()
    {
        return View(new FloorPlanUploadViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FloorPlanUploadViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Name is required.");
        }

        if (model.Image == null || model.Image.Length == 0)
        {
            ModelState.AddModelError(nameof(model.Image), "Please choose an image to upload.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var extension = Path.GetExtension(model.Image!.FileName);
        var allowed = new[] { ".png", ".jpg", ".jpeg" };
        if (!allowed.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.Image), "Only PNG or JPG images are allowed.");
            return View(model);
        }

        var folder = Path.Combine(environment.WebRootPath, "floorplans");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(folder, fileName);
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await model.Image.CopyToAsync(stream);
        }

        var floorPlan = new FloorPlan
        {
            Name = model.Name.Trim(),
            ImagePath = $"/floorplans/{fileName}"
        };

        context.FloorPlans.Add(floorPlan);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var plan = await context.FloorPlans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null)
        {
            return NotFound();
        }

        return View(plan);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var plan = await context.FloorPlans.FindAsync(id);
        if (plan != null)
        {
            var filePath = Path.Combine(environment.WebRootPath, plan.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            context.FloorPlans.Remove(plan);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
