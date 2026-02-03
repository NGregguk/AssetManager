using asset_manager.Data;
using asset_manager.Models;
using asset_manager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace asset_manager.Controllers;

[Authorize]
public class AssetsController(ApplicationDbContext context, IWebHostEnvironment environment) : Controller
{
    private static readonly string[] ServerCategoryNames = { "Servers", "Server" };

    public async Task<IActionResult> Index(string? q, int? status, int? categoryId, int? locationId, int? vendorId, bool? warrantyExpiring, bool? maintenanceDue, string? sort)
    {
        var assetsQuery = BuildAssetsQuery(q, status, categoryId, locationId, vendorId, warrantyExpiring, maintenanceDue);

        if (!categoryId.HasValue)
        {
            var serverCategory = await GetServerCategoryAsync();
            if (serverCategory != null)
            {
                assetsQuery = assetsQuery.Where(a => a.CategoryId == null || a.CategoryId != serverCategory.Id);
            }
        }

        assetsQuery = ApplySort(assetsQuery, sort);

        var assets = await assetsQuery.ToListAsync();

        ViewData["Query"] = q ?? string.Empty;
        ViewData["Status"] = status?.ToString() ?? string.Empty;
        ViewData["CategoryId"] = categoryId?.ToString() ?? string.Empty;
        ViewData["LocationId"] = locationId?.ToString() ?? string.Empty;
        ViewData["VendorId"] = vendorId?.ToString() ?? string.Empty;
        ViewData["WarrantyExpiring"] = warrantyExpiring == true ? "true" : string.Empty;
        ViewData["MaintenanceDue"] = maintenanceDue == true ? "true" : string.Empty;
        ViewData["Sort"] = sort ?? string.Empty;
        ViewData["ResultCount"] = assets.Count;

        ViewData["CategoryFilter"] = new SelectList(
            await context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(),
            "Id",
            "Name",
            categoryId);
        ViewData["LocationFilter"] = new SelectList(
            await context.Locations.AsNoTracking().OrderBy(l => l.Name).ToListAsync(),
            "Id",
            "Name",
            locationId);
        ViewData["VendorFilter"] = new SelectList(
            await context.Vendors.AsNoTracking().OrderBy(v => v.Name).ToListAsync(),
            "Id",
            "Name",
            vendorId);

        return View(assets);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export(string? q, int? status, int? categoryId, int? locationId, int? vendorId, bool? warrantyExpiring, bool? maintenanceDue, string? sort)
    {
        var assetsQuery = BuildAssetsQuery(q, status, categoryId, locationId, vendorId, warrantyExpiring, maintenanceDue);
        if (!categoryId.HasValue)
        {
            var serverCategory = await GetServerCategoryAsync();
            if (serverCategory != null)
            {
                assetsQuery = assetsQuery.Where(a => a.CategoryId == null || a.CategoryId != serverCategory.Id);
            }
        }
        assetsQuery = ApplySort(assetsQuery, sort);

        var assets = await assetsQuery.ToListAsync();
        var csv = BuildAssetCsv(assets);
        var fileName = $"assets-{DateTime.UtcNow:yyyyMMdd-HHmm}.csv";
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
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
            .Include(a => a.AssetModel)
            .Include(a => a.Attachments)
            .Include(a => a.Activities.OrderByDescending(x => x.CreatedAt))
            .Include(a => a.Pins)
            .Include(a => a.Assignments.OrderByDescending(x => x.AssignedDate))
            .Include(a => a.MaintenanceRecords.OrderByDescending(x => x.MaintenanceDate))
            .FirstOrDefaultAsync(m => m.Id == id);

        if (asset == null)
        {
            return NotFound();
        }

        return View(asset);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create(int? categoryId)
    {
        PopulateSelectLists(categoryId.HasValue ? new Asset { CategoryId = categoryId } : null);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([Bind("Name,Tag,SerialNumber,ComputerName,Architecture,BiosSerial,BiosVersion,Cores,Cpu,InstallDate,LogicalProcessors,Manufacturer,Model,OperatingSystem,OsBuild,OsVersion,Space,TotalRamGb,PurchaseDate,PurchaseCost,WarrantyExpiry,Description,Status,CategoryId,LocationId,VendorId,AssetModelId")] Asset asset)
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

    [Authorize(Roles = "Admin")]
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
        if (User.IsInRole("Admin"))
        {
            var activeAssignment = await context.Assignments.AsNoTracking()
                .Where(a => a.AssetId == asset.Id && a.ReturnedDate == null)
                .OrderByDescending(a => a.AssignedDate)
                .FirstOrDefaultAsync();
            ViewData["AssignedTo"] = activeAssignment?.AssignedTo ?? string.Empty;
            ViewData["ExpectedReturnDate"] = activeAssignment?.ExpectedReturnDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            ViewData["ReturnedDate"] = string.Empty;
        }
        return View(asset);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Tag,SerialNumber,ComputerName,Architecture,BiosSerial,BiosVersion,Cores,Cpu,InstallDate,LogicalProcessors,Manufacturer,Model,OperatingSystem,OsBuild,OsVersion,Space,TotalRamGb,PurchaseDate,PurchaseCost,WarrantyExpiry,Description,Status,CategoryId,LocationId,VendorId,AssetModelId")] Asset asset, string? assignedTo, DateOnly? expectedReturnDate, DateOnly? returnedDate)
    {
        if (id != asset.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            PopulateSelectLists(asset);
            ViewData["AssignedTo"] = assignedTo ?? string.Empty;
            ViewData["ExpectedReturnDate"] = expectedReturnDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            ViewData["ReturnedDate"] = returnedDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            return View(asset);
        }

        try
        {
            if (User.IsInRole("Admin"))
            {
                var activityEvents = await ApplyAssignmentUpdateAsync(asset, assignedTo, expectedReturnDate, returnedDate);
                AddActivityEvents(asset.Id, activityEvents, GetActorName());
            }

            AddActivity(asset.Id, AssetActivityType.AssetUpdated, "Asset details updated.", GetActorName());
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

    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadAttachment(int id, IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var asset = await context.Assets.FindAsync(id);
        if (asset == null)
        {
            return NotFound();
        }

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var folder = Path.Combine(environment.WebRootPath, "uploads", "assets", asset.Id.ToString());
        Directory.CreateDirectory(folder);
        var filePath = Path.Combine(folder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new AssetAttachment
        {
            AssetId = asset.Id,
            FilePath = $"/uploads/assets/{asset.Id}/{fileName}",
            OriginalFileName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            SizeBytes = file.Length
        };

        context.AssetAttachments.Add(attachment);
        AddActivity(asset.Id, AssetActivityType.AttachmentUploaded, $"Attachment uploaded: {attachment.OriginalFileName}.", GetActorName());
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = asset.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
    {
        var attachment = await context.AssetAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.AssetId == id);
        if (attachment == null)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var physicalPath = Path.Combine(environment.WebRootPath, attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(physicalPath))
        {
            System.IO.File.Delete(physicalPath);
        }

        context.AssetAttachments.Remove(attachment);
        AddActivity(id, AssetActivityType.AttachmentDeleted, $"Attachment deleted: {attachment.OriginalFileName}.", GetActorName());
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Checkout(int id)
    {
        var asset = await context.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        if (asset == null)
        {
            return NotFound();
        }

        var activeAssignment = await context.Assignments.AsNoTracking()
            .Where(a => a.AssetId == asset.Id && a.ReturnedDate == null)
            .OrderByDescending(a => a.AssignedDate)
            .FirstOrDefaultAsync();

        var viewModel = new AssetCheckoutViewModel
        {
            AssetId = asset.Id,
            AssetName = asset.Name,
            CurrentAssignee = activeAssignment?.AssignedTo,
            CurrentExpectedReturnDate = activeAssignment?.ExpectedReturnDate,
            AssignedTo = activeAssignment?.AssignedTo ?? string.Empty,
            ExpectedReturnDate = activeAssignment?.ExpectedReturnDate,
            ReturnedDate = null
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Checkout(AssetCheckoutViewModel model)
    {
        var asset = await context.Assets.FindAsync(model.AssetId);
        if (asset == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.AssetName = asset.Name;
            return View(model);
        }

        var activityEvents = await ApplyAssignmentUpdateAsync(asset, model.AssignedTo, model.ExpectedReturnDate, model.ReturnedDate);
        AddActivityEvents(asset.Id, activityEvents, GetActorName());
        context.Update(asset);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = asset.Id });
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Upload()
    {
        return View(new PcInfoUploadViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
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
        ViewData["AssetModels"] = context.AssetModels.AsNoTracking().OrderBy(m => m.Name).ToList();
    }

    public async Task<IActionResult> Servers(string? q, int? status, int? locationId, int? vendorId, bool? warrantyExpiring, bool? maintenanceDue, string? sort)
    {
        var serverCategory = await GetServerCategoryAsync();
        ViewData["ServerCategoryName"] = serverCategory?.Name ?? "Servers";
        ViewData["ServerCategoryId"] = serverCategory?.Id.ToString() ?? string.Empty;
        ViewData["MissingServerCategory"] = serverCategory == null;

        var assets = new List<Asset>();
        if (serverCategory != null)
        {
            var assetsQuery = BuildAssetsQuery(q, status, serverCategory.Id, locationId, vendorId, warrantyExpiring, maintenanceDue);
            assetsQuery = ApplySort(assetsQuery, sort);
            assets = await assetsQuery.ToListAsync();
        }

        ViewData["Query"] = q ?? string.Empty;
        ViewData["Status"] = status?.ToString() ?? string.Empty;
        ViewData["LocationId"] = locationId?.ToString() ?? string.Empty;
        ViewData["VendorId"] = vendorId?.ToString() ?? string.Empty;
        ViewData["WarrantyExpiring"] = warrantyExpiring == true ? "true" : string.Empty;
        ViewData["MaintenanceDue"] = maintenanceDue == true ? "true" : string.Empty;
        ViewData["Sort"] = sort ?? string.Empty;
        ViewData["ResultCount"] = assets.Count;

        ViewData["LocationFilter"] = new SelectList(
            await context.Locations.AsNoTracking().OrderBy(l => l.Name).ToListAsync(),
            "Id",
            "Name",
            locationId);
        ViewData["VendorFilter"] = new SelectList(
            await context.Vendors.AsNoTracking().OrderBy(v => v.Name).ToListAsync(),
            "Id",
            "Name",
            vendorId);

        return View(assets);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportServers(string? q, int? status, int? locationId, int? vendorId, bool? warrantyExpiring, bool? maintenanceDue, string? sort)
    {
        var serverCategory = await GetServerCategoryAsync();
        if (serverCategory == null)
        {
            return NotFound();
        }

        var assetsQuery = BuildAssetsQuery(q, status, serverCategory.Id, locationId, vendorId, warrantyExpiring, maintenanceDue);
        assetsQuery = ApplySort(assetsQuery, sort);

        var assets = await assetsQuery.ToListAsync();
        var csv = BuildAssetCsv(assets);
        var fileName = $"servers-{DateTime.UtcNow:yyyyMMdd-HHmm}.csv";
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
    }

    private bool AssetExists(int id)
    {
        return context.Assets.Any(e => e.Id == id);
    }

    private async Task<List<ActivityEvent>> ApplyAssignmentUpdateAsync(Asset asset, string? assignedTo, DateOnly? expectedReturnDate, DateOnly? returnedDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var trimmedAssignee = string.IsNullOrWhiteSpace(assignedTo) ? null : assignedTo.Trim();

        var activeAssignment = await context.Assignments
            .Where(a => a.AssetId == asset.Id && a.ReturnedDate == null)
            .OrderByDescending(a => a.AssignedDate)
            .FirstOrDefaultAsync();

        var previousAssignee = activeAssignment?.AssignedTo;
        var previousExpectedReturn = activeAssignment?.ExpectedReturnDate;

        var closedAssignment = false;
        var createdAssignment = false;
        var expectedReturnUpdated = false;

        if (trimmedAssignee != null)
        {
            if (activeAssignment == null)
            {
                context.Assignments.Add(new Assignment
                {
                    AssetId = asset.Id,
                    AssignedTo = trimmedAssignee,
                    AssignedDate = today,
                    ExpectedReturnDate = expectedReturnDate
                });
                createdAssignment = true;
            }
            else if (!string.Equals(activeAssignment.AssignedTo, trimmedAssignee, StringComparison.OrdinalIgnoreCase))
            {
                activeAssignment.ReturnedDate = returnedDate ?? today;
                closedAssignment = true;
                context.Assignments.Add(new Assignment
                {
                    AssetId = asset.Id,
                    AssignedTo = trimmedAssignee,
                    AssignedDate = today,
                    ExpectedReturnDate = expectedReturnDate
                });
                createdAssignment = true;
            }
            else
            {
                if (expectedReturnDate.HasValue)
                {
                    if (previousExpectedReturn != expectedReturnDate.Value)
                    {
                        activeAssignment.ExpectedReturnDate = expectedReturnDate.Value;
                        expectedReturnUpdated = true;
                    }
                }
                if (returnedDate.HasValue)
                {
                    activeAssignment.ReturnedDate = returnedDate.Value;
                    closedAssignment = true;
                }
            }
        }
        else if (activeAssignment != null)
        {
            activeAssignment.ReturnedDate = returnedDate ?? today;
            closedAssignment = true;
        }

        if (closedAssignment && !createdAssignment && asset.Status != AssetStatus.Retired)
        {
            asset.Status = AssetStatus.InStock;
        }

        if (createdAssignment && asset.Status != AssetStatus.Retired)
        {
            asset.Status = AssetStatus.Assigned;
        }

        var events = new List<ActivityEvent>();
        if (closedAssignment && !createdAssignment)
        {
            var who = string.IsNullOrWhiteSpace(previousAssignee) ? "current assignee" : previousAssignee;
            events.Add(new ActivityEvent(AssetActivityType.CheckedIn, $"Checked in from {who}."));
        }

        if (createdAssignment && trimmedAssignee != null)
        {
            var due = expectedReturnDate.HasValue ? $" Expected return {expectedReturnDate.Value:dd MMM yyyy}." : string.Empty;
            events.Add(new ActivityEvent(AssetActivityType.CheckedOut, $"Checked out to {trimmedAssignee}.{due}"));
        }

        if (expectedReturnUpdated && !createdAssignment && expectedReturnDate.HasValue)
        {
            events.Add(new ActivityEvent(AssetActivityType.ExpectedReturnUpdated, $"Expected return updated to {expectedReturnDate.Value:dd MMM yyyy}."));
        }

        return events;
    }

    private sealed record ActivityEvent(AssetActivityType Type, string Message);

    private void AddActivity(int assetId, AssetActivityType type, string message, string? performedBy)
    {
        context.AssetActivities.Add(new AssetActivity
        {
            AssetId = assetId,
            ActivityType = type,
            Message = message,
            PerformedBy = performedBy
        });
    }

    private void AddActivityEvents(int assetId, IEnumerable<ActivityEvent> events, string? performedBy)
    {
        foreach (var item in events)
        {
            AddActivity(assetId, item.Type, item.Message, performedBy);
        }
    }

    private string GetActorName()
    {
        return User?.Identity?.Name ?? "System";
    }

    private IQueryable<Asset> BuildAssetsQuery(string? q, int? status, int? categoryId, int? locationId, int? vendorId, bool? warrantyExpiring, bool? maintenanceDue)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var warrantyWindow = today.AddDays(90);
        var maintenanceWindow = today.AddDays(30);

        var assetsQuery = context.Assets
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Location)
            .Include(a => a.Vendor)
            .Include(a => a.Pins)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            assetsQuery = assetsQuery.Where(a =>
                a.Name.Contains(term) ||
                (a.Tag != null && a.Tag.Contains(term)) ||
                (a.SerialNumber != null && a.SerialNumber.Contains(term)) ||
                (a.ComputerName != null && a.ComputerName.Contains(term)));
        }

        if (status.HasValue)
        {
            assetsQuery = assetsQuery.Where(a => (int)a.Status == status.Value);
        }

        if (categoryId.HasValue)
        {
            assetsQuery = assetsQuery.Where(a => a.CategoryId == categoryId.Value);
        }

        if (locationId.HasValue)
        {
            assetsQuery = assetsQuery.Where(a => a.LocationId == locationId.Value);
        }

        if (vendorId.HasValue)
        {
            assetsQuery = assetsQuery.Where(a => a.VendorId == vendorId.Value);
        }

        if (warrantyExpiring == true)
        {
            assetsQuery = assetsQuery.Where(a =>
                a.WarrantyExpiry != null &&
                a.WarrantyExpiry >= today &&
                a.WarrantyExpiry <= warrantyWindow);
        }

        if (maintenanceDue == true)
        {
            assetsQuery = assetsQuery.Where(a =>
                a.MaintenanceRecords.Any(m => m.NextDueDate != null && m.NextDueDate <= maintenanceWindow));
        }

        return assetsQuery;
    }

    private static IQueryable<Asset> ApplySort(IQueryable<Asset> assetsQuery, string? sort)
    {
        return sort switch
        {
            "name_desc" => assetsQuery.OrderByDescending(a => a.Name),
            "purchase_desc" => assetsQuery.OrderByDescending(a => a.PurchaseDate),
            "purchase_asc" => assetsQuery.OrderBy(a => a.PurchaseDate),
            "warranty_asc" => assetsQuery.OrderBy(a => a.WarrantyExpiry),
            "warranty_desc" => assetsQuery.OrderByDescending(a => a.WarrantyExpiry),
            "status" => assetsQuery.OrderBy(a => a.Status),
            "category" => assetsQuery.OrderBy(a => a.Category == null ? string.Empty : a.Category.Name),
            _ => assetsQuery.OrderBy(a => a.Name)
        };
    }

    private static string BuildAssetCsv(IEnumerable<Asset> assets)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Name,Tag,SerialNumber,ComputerName,Status,Category,Location,Vendor,PurchaseDate,WarrantyExpiry,PurchaseCost");

        foreach (var asset in assets)
        {
            sb.AppendLine(string.Join(",",
                EscapeCsv(asset.Name),
                EscapeCsv(asset.Tag),
                EscapeCsv(asset.SerialNumber),
                EscapeCsv(asset.ComputerName),
                EscapeCsv(asset.Status.ToString()),
                EscapeCsv(asset.Category?.Name),
                EscapeCsv(asset.Location?.Name),
                EscapeCsv(asset.Vendor?.Name),
                EscapeCsv(asset.PurchaseDate?.ToString("yyyy-MM-dd")),
                EscapeCsv(asset.WarrantyExpiry?.ToString("yyyy-MM-dd")),
                EscapeCsv(asset.PurchaseCost?.ToString("0.##"))
            ));
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        var escaped = value.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{escaped}\"" : escaped;
    }

    private Task<Category?> GetServerCategoryAsync()
    {
        return context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .FirstOrDefaultAsync(c => c.Name != null && ServerCategoryNames.Contains(c.Name));
    }
}
