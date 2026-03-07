using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using InventoryApp.Models;

namespace InventoryApp.Controllers;

public class ItemsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ItemsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Helper: Verify if user has Write Access for a given Inventory
    private async Task<bool> UserHasWriteAccessAsync(Inventory inventory)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return false;

        bool isOwnerOrAdmin = inventory.CreatorId == user.Id || User.IsInRole("Admin");
        if (isOwnerOrAdmin) return true;

        if (inventory.IsPublic) return true;

        // Check explicit access table
        return inventory.Accesses.Any(a => a.UserId == user.Id);
    }

    // GET: Items/Create?inventoryId=5
    [Authorize]
    public async Task<IActionResult> Create(int inventoryId)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Accesses)
            .FirstOrDefaultAsync(i => i.Id == inventoryId);

        if (inventory == null) return NotFound();

        if (!await UserHasWriteAccessAsync(inventory))
            return Forbid();

        var item = new Item { InventoryId = inventoryId, Inventory = inventory };
        return View(item);
    }

    // POST: Items/Create
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Item item)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Accesses)
            .FirstOrDefaultAsync(i => i.Id == item.InventoryId);

        if (inventory == null) return NotFound();

        if (!await UserHasWriteAccessAsync(inventory))
            return Forbid();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        item.CreatedById = user.Id;
        item.CreatedAt = DateTime.UtcNow;

        ModelState.Remove("CreatedBy");
        ModelState.Remove("Inventory");
        ModelState.Remove("CustomId"); // We generate this

        if (ModelState.IsValid)
        {
            // Transient CustomId Generation Retry Loop
            bool saved = false;
            int attempt = 0;
            while (!saved && attempt < 5)
            {
                attempt++;
                string candidate = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                item.CustomId = candidate;

                // Simple check before hitting DB Unique constraint
                if (!await _context.Items.AnyAsync(i => i.InventoryId == item.InventoryId && i.CustomId == item.CustomId))
                {
                    try
                    {
                        _context.Add(item);
                        await _context.SaveChangesAsync();
                        saved = true;
                    }
                    catch (DbUpdateException)
                    {
                        // DB Unique constraint hit because of concurrent saves, detach and retry
                        _context.Entry(item).State = EntityState.Detached;
                    }
                }
            }

            if (saved)
            {
                return RedirectToAction("Details", "Inventories", new { id = item.InventoryId, tab = "items" });
            }
            else
            {
                ModelState.AddModelError("", "Failed to generate a unique Custom ID after multiple attempts. Please try again.");
            }
        }

        item.Inventory = inventory; // Restore for view
        return View(item);
    }

    // GET: Items/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _context.Items
            .Include(i => i.Inventory)
                .ThenInclude(inv => inv!.Accesses)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null || item.Inventory == null) return NotFound();

        if (!await UserHasWriteAccessAsync(item.Inventory))
            return Forbid();

        return View(item);
    }

    // POST: Items/Edit/5
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Item itemData)
    {
        if (id != itemData.Id) return NotFound();

        var dbItem = await _context.Items
            .Include(i => i.Inventory)
                .ThenInclude(inv => inv!.Accesses)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (dbItem == null || dbItem.Inventory == null) return NotFound();

        if (!await UserHasWriteAccessAsync(dbItem.Inventory))
            return Forbid();

        // Map values manually to prevent mass-assignment vulnerabilities
        dbItem.String1 = itemData.String1;
        dbItem.String2 = itemData.String2;
        dbItem.String3 = itemData.String3;
        dbItem.Text1 = itemData.Text1;
        dbItem.Text2 = itemData.Text2;
        dbItem.Text3 = itemData.Text3;
        dbItem.Int1 = itemData.Int1;
        dbItem.Int2 = itemData.Int2;
        dbItem.Int3 = itemData.Int3;
        dbItem.Bool1 = itemData.Bool1;
        dbItem.Bool2 = itemData.Bool2;
        dbItem.Bool3 = itemData.Bool3;
        dbItem.Link1 = itemData.Link1;
        dbItem.Link2 = itemData.Link2;
        dbItem.Link3 = itemData.Link3;

        // Optimistic locking
        _context.Entry(dbItem).Property(i => i.Version).OriginalValue = itemData.Version;

        ModelState.Clear(); // we mapped manually, let's just attempt save

        try
        {
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Inventories", new { id = dbItem.InventoryId, tab = "items" });
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError("Version", "This item was modified by another user. Please refresh and try again.");
            // We need to return to View to show error
            itemData.Inventory = dbItem.Inventory;
            return View(itemData);
        }
    }

    // POST: Items/DeleteSelected
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSelected(int inventoryId, int[] itemIds)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Accesses)
            .FirstOrDefaultAsync(i => i.Id == inventoryId);

        if (inventory == null) return NotFound();

        if (!await UserHasWriteAccessAsync(inventory))
            return Forbid();

        if (itemIds != null && itemIds.Length > 0)
        {
            var itemsToDelete = await _context.Items
                .Where(i => i.InventoryId == inventoryId && itemIds.Contains(i.Id))
                .ToListAsync();

            _context.Items.RemoveRange(itemsToDelete);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", "Inventories", new { id = inventoryId, tab = "items" });
    }
}
