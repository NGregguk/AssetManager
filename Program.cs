using asset_manager.Data;
using asset_manager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true)
        .Build())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

await SeedUsersAsync(app.Services, app.Configuration);
await SeedSampleDataAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapRazorPages();

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static async Task SeedUsersAsync(IServiceProvider services, IConfiguration configuration)
{
    using var scope = services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var seedSection = configuration.GetSection("SeedUsers");
    if (!seedSection.Exists())
    {
        return;
    }

    foreach (var child in seedSection.GetChildren())
    {
        var email = child.GetValue<string>("Email");
        var password = child.GetValue<string>("Password");
        var role = child.GetValue<string>("Role");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            continue;
        }

        if (!string.IsNullOrWhiteSpace(role) && !await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                continue;
            }
        }

        if (!string.IsNullOrWhiteSpace(role) && !await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}

static async Task SeedSampleDataAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (await context.Assets.AnyAsync())
    {
        return;
    }

    var categories = new[]
    {
        new Category { Name = "Laptops" },
        new Category { Name = "Displays" },
        new Category { Name = "Networking" },
        new Category { Name = "Mobile" }
    };

    var locations = new[]
    {
        new Location { Name = "HQ - London" },
        new Location { Name = "Warehouse" },
        new Location { Name = "Remote" }
    };

    var vendors = new[]
    {
        new Vendor { Name = "Dell" },
        new Vendor { Name = "HP" },
        new Vendor { Name = "Cisco" },
        new Vendor { Name = "Apple" }
    };

    context.Categories.AddRange(categories);
    context.Locations.AddRange(locations);
    context.Vendors.AddRange(vendors);
    await context.SaveChangesAsync();

    var assets = new[]
    {
        new Asset
        {
            Name = "Latitude 7440",
            Tag = "LT-0042",
            SerialNumber = "DL7440-8821",
            PurchaseDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-10)),
            PurchaseCost = 1499m,
            WarrantyExpiry = DateOnly.FromDateTime(DateTime.Today.AddMonths(14)),
            Description = "Sales team laptop",
            Status = AssetStatus.Assigned,
            CategoryId = categories[0].Id,
            LocationId = locations[2].Id,
            VendorId = vendors[0].Id
        },
        new Asset
        {
            Name = "EliteBook 860",
            Tag = "LT-0099",
            SerialNumber = "HP860-2290",
            PurchaseDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-18)),
            PurchaseCost = 1299m,
            WarrantyExpiry = DateOnly.FromDateTime(DateTime.Today.AddMonths(6)),
            Description = "Engineering laptop",
            Status = AssetStatus.Assigned,
            CategoryId = categories[0].Id,
            LocationId = locations[0].Id,
            VendorId = vendors[1].Id
        },
        new Asset
        {
            Name = "UltraSharp 27\"",
            Tag = "DS-0144",
            SerialNumber = "U2723QE-4411",
            PurchaseDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-8)),
            PurchaseCost = 699m,
            WarrantyExpiry = DateOnly.FromDateTime(DateTime.Today.AddMonths(28)),
            Description = "Design team display",
            Status = AssetStatus.InStock,
            CategoryId = categories[1].Id,
            LocationId = locations[1].Id,
            VendorId = vendors[0].Id
        },
        new Asset
        {
            Name = "Cisco Meraki MX",
            Tag = "NW-0021",
            SerialNumber = "MX-550-991",
            PurchaseDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-30)),
            PurchaseCost = 3899m,
            WarrantyExpiry = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            Description = "Primary firewall",
            Status = AssetStatus.Assigned,
            CategoryId = categories[2].Id,
            LocationId = locations[0].Id,
            VendorId = vendors[2].Id
        },
        new Asset
        {
            Name = "iPhone 15 Pro",
            Tag = "MB-0301",
            SerialNumber = "APL15P-7731",
            PurchaseDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-4)),
            PurchaseCost = 1099m,
            WarrantyExpiry = DateOnly.FromDateTime(DateTime.Today.AddMonths(20)),
            Description = "Customer success device",
            Status = AssetStatus.Assigned,
            CategoryId = categories[3].Id,
            LocationId = locations[2].Id,
            VendorId = vendors[3].Id
        }
    };

    context.Assets.AddRange(assets);
    await context.SaveChangesAsync();

    var assignments = new[]
    {
        new Assignment
        {
            AssetId = assets[0].Id,
            AssignedTo = "Sasha Brooks",
            AssignedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-45))
        },
        new Assignment
        {
            AssetId = assets[1].Id,
            AssignedTo = "Imran Ahmed",
            AssignedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-110))
        },
        new Assignment
        {
            AssetId = assets[4].Id,
            AssignedTo = "Priya Nair",
            AssignedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-12))
        }
    };

    var maintenance = new[]
    {
        new MaintenanceRecord
        {
            AssetId = assets[0].Id,
            VendorId = vendors[0].Id,
            MaintenanceDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-20)),
            NextDueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(160)),
            Summary = "Quarterly health check"
        },
        new MaintenanceRecord
        {
            AssetId = assets[3].Id,
            VendorId = vendors[2].Id,
            MaintenanceDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-200)),
            NextDueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)),
            Summary = "Firmware update and inspection"
        },
        new MaintenanceRecord
        {
            AssetId = assets[2].Id,
            VendorId = vendors[0].Id,
            MaintenanceDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-35)),
            NextDueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(80)),
            Summary = "Calibration and cleanup"
        }
    };

    context.Assignments.AddRange(assignments);
    context.MaintenanceRecords.AddRange(maintenance);
    await context.SaveChangesAsync();
}
