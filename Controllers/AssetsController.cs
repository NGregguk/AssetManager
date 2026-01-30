using asset_manager.Data;
using asset_manager.Models;
using asset_manager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace asset_manager.Controllers;

[Authorize]
public class AssetsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var assets = await context.Assets
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Location)
            .Include(a => a.Vendor)
            .OrderBy(a => a.Name)
            .ToListAsync();

        return View(assets);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asset = await context.Assets
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Location)
            .Include(a => a.Vendor)
            .Include(a => a.Assignments.OrderByDescending(x => x.AssignedDate))
            .Include(a => a.MaintenanceRecords.OrderByDescending(x => x.MaintenanceDate))
            .FirstOrDefaultAsync(m => m.Id == id);

        if (asset == null)
        {
            return NotFound();
        }

        return View(asset);
    }

    public IActionResult Create()
    {
        PopulateSelectLists();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Tag,SerialNumber,ComputerName,Architecture,BiosSerial,BiosVersion,Cores,Cpu,InstallDate,LogicalProcessors,Manufacturer,Model,OperatingSystem,OsBuild,OsVersion,Space,TotalRamGb,PurchaseDate,PurchaseCost,WarrantyExpiry,Description,Status,CategoryId,LocationId,VendorId")] Asset asset)
    {
        if (!ModelState.IsValid)
        {
            PopulateSelectLists(asset);
            return View(asset);
        }

        context.Add(asset);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asset = await context.Assets.FindAsync(id);
        if (asset == null)
        {
            return NotFound();
        }

        PopulateSelectLists(asset);
        return View(asset);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Tag,SerialNumber,ComputerName,Architecture,BiosSerial,BiosVersion,Cores,Cpu,InstallDate,LogicalProcessors,Manufacturer,Model,OperatingSystem,OsBuild,OsVersion,Space,TotalRamGb,PurchaseDate,PurchaseCost,WarrantyExpiry,Description,Status,CategoryId,LocationId,VendorId")] Asset asset)
    {
        if (id != asset.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            PopulateSelectLists(asset);
            return View(asset);
        }

        try
        {
            context.Update(asset);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AssetExists(asset.Id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asset = await context.Assets
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Location)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (asset == null)
        {
            return NotFound();
        }

        return View(asset);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var asset = await context.Assets.FindAsync(id);
        if (asset != null)
        {
            context.Assets.Remove(asset);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Assignments(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asset = await context.Assets
            .AsNoTracking()
            .Include(a => a.Assignments.OrderByDescending(x => x.AssignedDate))
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null)
        {
            return NotFound();
        }

        return View(asset);
    }

    public async Task<IActionResult> Maintenance(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asset = await context.Assets
            .AsNoTracking()
            .Include(a => a.MaintenanceRecords.OrderByDescending(x => x.MaintenanceDate))
            .ThenInclude(m => m.Vendor)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null)
        {
            return NotFound();
        }

        return View(asset);
    }

    public IActionResult Upload()
    {
        return View(new PcInfoUploadViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(PcInfoUploadViewModel model)
    {
        if (model.File == null || model.File.Length == 0)
        {
            ModelState.AddModelError(nameof(model.File), "Please select a CSV file to upload.");
            return View(model);
        }

        var result = new PcInfoUploadViewModel();
        var existingAssets = await context.Assets.AsNoTracking()
            .Select(a => new { a.Id, a.ComputerName, a.BiosSerial })
            .ToListAsync();

        using var stream = model.File.OpenReadStream();
        using var reader = new StreamReader(stream);
        var headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            ModelState.AddModelError(nameof(model.File), "CSV file is empty.");
            return View(model);
        }

        var headers = SplitCsvLine(headerLine);
        var headerMap = headers
            .Select((name, index) => new { name = name.Trim(), index })
            .ToDictionary(x => x.name, x => x.index, StringComparer.OrdinalIgnoreCase);

        var requiredColumns = new[] { "Computer_Name", "BIOS_Serial" };
        if (!requiredColumns.Any(c => headerMap.ContainsKey(c)))
        {
            ModelState.AddModelError(nameof(model.File), "CSV header missing expected columns (Computer_Name or BIOS_Serial).");
            return View(model);
        }

        var assetsToAdd = new List<Asset>();
        var rowIndex = 1;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            rowIndex++;
            if (string.IsNullOrWhiteSpace(line))
            {
                result.Skipped++;
                continue;
            }

            var fields = SplitCsvLine(line);
            string GetValue(string columnName)
            {
                return headerMap.TryGetValue(columnName, out var idx) && idx < fields.Length
                    ? fields[idx].Trim()
                    : string.Empty;
            }

            var computerName = GetValue("Computer_Name");
            var biosSerial = GetValue("BIOS_Serial");

            if (string.IsNullOrWhiteSpace(computerName) && string.IsNullOrWhiteSpace(biosSerial))
            {
                result.Skipped++;
                continue;
            }

            if (existingAssets.Any(a =>
                    (!string.IsNullOrWhiteSpace(computerName) && string.Equals(a.ComputerName, computerName, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(biosSerial) && string.Equals(a.BiosSerial, biosSerial, StringComparison.OrdinalIgnoreCase))))
            {
                result.Skipped++;
                continue;
            }

            var asset = new Asset
            {
                Name = string.IsNullOrWhiteSpace(computerName) ? "Imported device" : computerName,
                ComputerName = computerName,
                Architecture = GetValue("Architecture"),
                BiosSerial = biosSerial,
                BiosVersion = GetValue("BIOS_Version"),
                Cpu = GetValue("CPU"),
                Manufacturer = GetValue("Manufacturer"),
                Model = GetValue("Model"),
                OperatingSystem = GetValue("Operating_System"),
                OsBuild = GetValue("OS_Build"),
                OsVersion = GetValue("OS_Version"),
                Space = GetValue("Space"),
                Status = AssetStatus.InStock
            };

            if (int.TryParse(GetValue("Cores"), out var cores))
            {
                asset.Cores = cores;
            }

            if (int.TryParse(GetValue("Logical_Processors"), out var logicalProcessors))
            {
                asset.LogicalProcessors = logicalProcessors;
            }

            if (DateOnly.TryParse(GetValue("Install_Date"), out var installDate))
            {
                asset.InstallDate = installDate;
            }

            if (decimal.TryParse(GetValue("Total_RAM_GB"), out var ramGb))
            {
                asset.TotalRamGb = ramGb;
            }
            else if (!string.IsNullOrWhiteSpace(GetValue("Total_RAM_GB")))
            {
                result.Errors.Add($"Row {rowIndex}: Total_RAM_GB value '{GetValue("Total_RAM_GB")}' is invalid.");
            }

            assetsToAdd.Add(asset);
            result.Imported++;
        }

        if (assetsToAdd.Count > 0)
        {
            context.Assets.AddRange(assetsToAdd);
            await context.SaveChangesAsync();
        }

        return View(result);
    }

    private static string[] SplitCsvLine(string line)
    {
        return line.Split(',', StringSplitOptions.TrimEntries);
    }

    private void PopulateSelectLists(Asset? asset = null)
    {
        ViewData["CategoryId"] = new SelectList(context.Categories.OrderBy(c => c.Name), "Id", "Name", asset?.CategoryId);
        ViewData["LocationId"] = new SelectList(context.Locations.OrderBy(l => l.Name), "Id", "Name", asset?.LocationId);
        ViewData["VendorId"] = new SelectList(context.Vendors.OrderBy(v => v.Name), "Id", "Name", asset?.VendorId);
    }

    private bool AssetExists(int id)
    {
        return context.Assets.Any(e => e.Id == id);
    }
}
