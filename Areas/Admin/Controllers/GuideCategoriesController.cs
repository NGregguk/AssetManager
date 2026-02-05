using asset_manager.Data;
using asset_manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace asset_manager.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class GuideCategoriesController(ApplicationDbContext context, IWebHostEnvironment environment) : Controller
{
    private static readonly Regex GuideImageRegex = new("src\\s*=\\s*(\"|')(?<path>/guides/images/[^\"']+)\\1", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<IActionResult> Index()
    {
        var items = await context.GuideCategories.AsNoTracking()
            .Include(c => c.Guides)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
        return View(items);
    }

    public IActionResult Create()
    {
        return View(new GuideCategory());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description,SortOrder")] GuideCategory category)
    {
        category.Name = category.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            ModelState.AddModelError(nameof(category.Name), "Name is required.");
        }

        if (!string.IsNullOrWhiteSpace(category.Description))
        {
            category.Description = category.Description.Trim();
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        context.GuideCategories.Add(category);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await context.GuideCategories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,SortOrder")] GuideCategory category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        category.Name = category.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            ModelState.AddModelError(nameof(category.Name), "Name is required.");
        }

        if (!string.IsNullOrWhiteSpace(category.Description))
        {
            category.Description = category.Description.Trim();
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        context.Update(category);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
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

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await context.GuideCategories
            .Include(c => c.Guides)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (category != null)
        {
            foreach (var guide in category.Guides)
            {
                DeleteInlineImages(guide.Content);

                if (!string.IsNullOrWhiteSpace(guide.AttachmentPath))
                {
                    DeleteAttachmentFile(guide.AttachmentPath);
                }
            }

            context.GuideCategories.Remove(category);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private void DeleteAttachmentFile(string attachmentPath)
    {
        var filePath = Path.Combine(environment.WebRootPath, attachmentPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }
    }

    private void DeleteInlineImages(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return;
        }

        foreach (Match match in GuideImageRegex.Matches(html))
        {
            var path = match.Groups["path"].Value;
            if (!string.IsNullOrWhiteSpace(path))
            {
                DeleteAttachmentFile(path);
            }
        }
    }
}
