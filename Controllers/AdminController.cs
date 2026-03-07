using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Models.ViewModels;

namespace InventoryApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public AdminController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var model = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var isBlocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
            
            model.Add(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                IsAdmin = roles.Contains("Admin"),
                IsBlocked = isBlocked
            });
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PerformAction(List<string> userIds, string actionType)
    {
        if (userIds == null || !userIds.Any())
        {
            TempData["ErrorMessage"] = "No users selected.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var userId in userIds)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) continue;

            switch (actionType)
            {
                case "Block":
                    await _userManager.SetLockoutEnabledAsync(user, true);
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                    await _userManager.UpdateSecurityStampAsync(user);
                    break;

                case "Unblock":
                    await _userManager.SetLockoutEndDateAsync(user, null);
                    await _userManager.UpdateSecurityStampAsync(user);
                    break;

                case "AddAdmin":
                    if (!await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                        await _userManager.UpdateSecurityStampAsync(user);
                    }
                    break;

                case "RemoveAdmin":
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        await _userManager.RemoveFromRoleAsync(user, "Admin");
                        await _userManager.UpdateSecurityStampAsync(user);
                    }
                    break;

                case "Delete":
                    await _userManager.DeleteAsync(user);
                    break;
            }
        }

        TempData["SuccessMessage"] = $"Action '{actionType}' completed successfully.";
        return RedirectToAction(nameof(Index));
    }
}
