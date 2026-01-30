using asset_manager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace asset_manager.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var users = userManager.Users.ToList();
        var viewModels = new List<AdminUserViewModel>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            viewModels.Add(new AdminUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? "Unknown",
                Role = roles.FirstOrDefault() ?? "User"
            });
        }

        return View(viewModels);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await userManager.GetRolesAsync(user);
        var availableRoles = await roleManager.Roles.Select(r => r.Name!).ToListAsync();
        if (!availableRoles.Contains("Admin"))
        {
            availableRoles.Add("Admin");
        }
        if (!availableRoles.Contains("User"))
        {
            availableRoles.Add("User");
        }

        return View(new AdminUserEditViewModel
        {
            Id = user.Id,
            Email = user.Email ?? user.UserName ?? "Unknown",
            SelectedRole = roles.FirstOrDefault() ?? "User",
            AvailableRoles = availableRoles
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, AdminUserEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var existingRoles = await userManager.GetRolesAsync(user);
        if (existingRoles.Count > 0)
        {
            await userManager.RemoveFromRolesAsync(user, existingRoles);
        }

        if (!string.IsNullOrWhiteSpace(model.SelectedRole))
        {
            if (!await roleManager.RoleExistsAsync(model.SelectedRole))
            {
                await roleManager.CreateAsync(new IdentityRole(model.SelectedRole));
            }

            await userManager.AddToRoleAsync(user, model.SelectedRole);
        }

        return RedirectToAction(nameof(Index));
    }
}
