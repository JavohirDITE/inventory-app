using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using InventoryApp.Models;

namespace InventoryApp.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();

        // My Inventories (Owned by user)
        var myInventories = await _context.Inventories
            .Include(i => i.Category)
            .Where(i => i.CreatorId == userId)
            .OrderByDescending(i => i.Id)
            .ToListAsync();

        // Shared With Me (User has access, but doesn't own it)
        var sharedInventories = await _context.InventoryAccesses
            .Include(ia => ia.Inventory)
            .ThenInclude(i => i.Category)
            .Where(ia => ia.UserId == userId && ia.Inventory!.CreatorId != userId)
            .Select(ia => ia.Inventory!)
            .OrderByDescending(i => i.Id)
            .ToListAsync();

        var model = new ProfileViewModel
        {
            MyInventories = myInventories,
            SharedInventories = sharedInventories
        };

        return View(model);
    }
}

public class ProfileViewModel
{
    public List<Inventory> MyInventories { get; set; } = new();
    public List<Inventory> SharedInventories { get; set; } = new();
}
