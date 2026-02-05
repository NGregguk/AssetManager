using asset_manager.Data;
using asset_manager.Models;
using asset_manager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace asset_manager.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class GuidesController(ApplicationDbContext context, IWebHostEnvironment environment) : Controller
{
    private static readonly string[] AllowedExtensions = [".pdf", ".doc", ".docx", ".txt", ".md"];
    private static readonly string[] AllowedImageExtensions = [".png", ".jpg", ".jpeg", ".gif", ".webp"];
    private static readonly Regex ScriptTagRegex = new("<script.*?>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex StyleTagRegex = new("<style.*?>.*?</style>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex OnEventRegex = new("\\son\\w+\\s*=\\s*(\"[^\"]*\"|'[^']*'|[^\\s>]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex JsProtocolRegex = new("(?i)(href|src)\\s*=\\s*(\"|')\\s*javascript:[^\"']*\\2", RegexOptions.Compiled);
    private static readonly Regex GuideImageRegex = new("src\\s*=\\s*(\"|')(?<path>/guides/images/[^\"']+)\\1", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<IActionResult> Index()
    {
        var categories = await context.GuideCategories.AsNoTracking()
            .Include(c => c.Guides)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return View(categories);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var guide = await context.Guides.AsNoTracking()
            .Include(g => g.GuideCategory)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (guide == null)
        {
            return NotFound();
        }

        return View(guide);
    }

    public async Task<IActionResult> Create(int? categoryId)
    {
        await PopulateCategoriesAsync(categoryId);
        return View(new GuideEditorViewModel { GuideCategoryId = categoryId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GuideEditorViewModel model)
    {
        await PopulateCategoriesAsync(model.GuideCategoryId);
        NormalizeModel(model);

        if (string.IsNullOrWhiteSpace(model.Title))
        {
            ModelState.AddModelError(nameof(model.Title), "Title is required.");
        }

        if (!await context.GuideCategories.AnyAsync(c => c.Id == model.GuideCategoryId))
        {
            ModelState.AddModelError(nameof(model.GuideCategoryId), "Please choose a valid category.");
        }

        if (!HasContentOrAttachment(model, hasExistingAttachment: false))
        {
            ModelState.AddModelError(nameof(model.Content), "Add guide content or upload a file.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var guide = new Guide
        {
            Title = model.Title,
            Summary = model.Summary,
            Content = SanitizeHtml(model.Content),
            GuideCategoryId = model.GuideCategoryId,
            SortOrder = model.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (model.Attachment != null && model.Attachment.Length > 0)
        {
            var attachment = await SaveAttachmentAsync(model.Attachment);
            if (attachment == null)
            {
                ModelState.AddModelError(nameof(model.Attachment), $"Allowed file types: {string.Join(", ", AllowedExtensions)}.");
                return View(model);
            }

            guide.AttachmentName = attachment.Value.FileName;
            guide.AttachmentPath = attachment.Value.Path;
        }

        context.Guides.Add(guide);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var guide = await context.Guides.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
        if (guide == null)
        {
            return NotFound();
        }

        await PopulateCategoriesAsync(guide.GuideCategoryId);

        var model = new GuideEditorViewModel
        {
            Id = guide.Id,
            Title = guide.Title,
            Summary = guide.Summary,
            Content = guide.Content,
            GuideCategoryId = guide.GuideCategoryId,
            SortOrder = guide.SortOrder,
            ExistingAttachmentName = guide.AttachmentName,
            ExistingAttachmentPath = guide.AttachmentPath
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GuideEditorViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        await PopulateCategoriesAsync(model.GuideCategoryId);
        NormalizeModel(model);

        if (string.IsNullOrWhiteSpace(model.Title))
        {
            ModelState.AddModelError(nameof(model.Title), "Title is required.");
        }

        var guide = await context.Guides.FirstOrDefaultAsync(g => g.Id == id);
        if (guide == null)
        {
            return NotFound();
        }

        if (!await context.GuideCategories.AnyAsync(c => c.Id == model.GuideCategoryId))
        {
            ModelState.AddModelError(nameof(model.GuideCategoryId), "Please choose a valid category.");
        }

        var hasExistingAttachment = !string.IsNullOrWhiteSpace(guide.AttachmentPath);
        if (!HasContentOrAttachment(model, hasExistingAttachment && !model.RemoveAttachment))
        {
            ModelState.AddModelError(nameof(model.Content), "Add guide content or upload a file.");
        }

        if (!ModelState.IsValid)
        {
            model.ExistingAttachmentName = guide.AttachmentName;
            model.ExistingAttachmentPath = guide.AttachmentPath;
            return View(model);
        }

        guide.Title = model.Title;
        guide.Summary = model.Summary;
        guide.Content = SanitizeHtml(model.Content);
        guide.GuideCategoryId = model.GuideCategoryId;
        guide.SortOrder = model.SortOrder;
        guide.UpdatedAt = DateTime.UtcNow;

        if (model.RemoveAttachment && !string.IsNullOrWhiteSpace(guide.AttachmentPath))
        {
            DeleteAttachmentFile(guide.AttachmentPath);
            guide.AttachmentPath = null;
            guide.AttachmentName = null;
        }

        if (model.Attachment != null && model.Attachment.Length > 0)
        {
            var attachment = await SaveAttachmentAsync(model.Attachment);
            if (attachment == null)
            {
                ModelState.AddModelError(nameof(model.Attachment), $"Allowed file types: {string.Join(", ", AllowedExtensions)}.");
                model.ExistingAttachmentName = guide.AttachmentName;
                model.ExistingAttachmentPath = guide.AttachmentPath;
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(guide.AttachmentPath))
            {
                DeleteAttachmentFile(guide.AttachmentPath);
            }

            guide.AttachmentName = attachment.Value.FileName;
            guide.AttachmentPath = attachment.Value.Path;
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Please choose an image to upload." });
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Only PNG, JPG, GIF, or WEBP images are allowed." });
        }

        var folder = Path.Combine(environment.WebRootPath, "guides", "images");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(folder, fileName);
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Json(new { link = $"/guides/images/{fileName}" });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var guide = await context.Guides.AsNoTracking()
            .Include(g => g.GuideCategory)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (guide == null)
        {
            return NotFound();
        }

        return View(guide);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var guide = await context.Guides.FirstOrDefaultAsync(g => g.Id == id);
        if (guide != null)
        {
            DeleteInlineImages(guide.Content);

            if (!string.IsNullOrWhiteSpace(guide.AttachmentPath))
            {
                DeleteAttachmentFile(guide.AttachmentPath);
            }

            context.Guides.Remove(guide);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategoriesAsync(int? selectedCategoryId)
    {
        var categories = await context.GuideCategories.AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        ViewBag.GuideCategoryId = new SelectList(categories, "Id", "Name", selectedCategoryId);
        ViewBag.GuideCategoryCount = categories.Count;
    }

    private void NormalizeModel(GuideEditorViewModel model)
    {
        model.Title = model.Title?.Trim() ?? string.Empty;
        model.Summary = string.IsNullOrWhiteSpace(model.Summary) ? null : model.Summary.Trim();
        model.Content = string.IsNullOrWhiteSpace(model.Content) ? null : model.Content.Trim();
    }

    private bool HasContentOrAttachment(GuideEditorViewModel model, bool hasExistingAttachment)
    {
        var hasContent = !string.IsNullOrWhiteSpace(model.Content);
        var hasNewAttachment = model.Attachment != null && model.Attachment.Length > 0;
        return hasContent || hasNewAttachment || hasExistingAttachment;
    }

    private async Task<(string FileName, string Path)?> SaveAttachmentAsync(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return null;
        }

        var folder = Path.Combine(environment.WebRootPath, "guides");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(folder, fileName);
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return (Path.GetFileName(file.FileName), $"/guides/{fileName}");
    }

    private void DeleteAttachmentFile(string attachmentPath)
    {
        var filePath = Path.Combine(environment.WebRootPath, attachmentPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }
    }

    private string? SanitizeHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var output = html;
        output = ScriptTagRegex.Replace(output, string.Empty);
        output = StyleTagRegex.Replace(output, string.Empty);
        output = OnEventRegex.Replace(output, string.Empty);
        output = JsProtocolRegex.Replace(output, "$1=$2#$2");
        return output.Trim();
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
