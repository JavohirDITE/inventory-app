using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using InventoryApp.Models;
using InventoryApp.Models.ViewModels;

namespace InventoryApp.Controllers;

public class InventoriesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public InventoriesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Inventories
    public async Task<IActionResult> Index()
    {
        // Read access is global. Anyone can see the list.
        var inventories = await _context.Inventories
            .Include(i => i.Category)
            .Include(i => i.Creator)
            .OrderByDescending(i => i.Id)
            .ToListAsync();
            
        return View(inventories);
    }

    // GET: Inventories/Details/5
    public async Task<IActionResult> Details(int? id, string tab = "items")
    {
        if (id == null) return NotFound();

        var inventory = await _context.Inventories
            .Include(i => i.Category)
            .Include(i => i.Creator)
            .Include(i => i.Accesses)
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (inventory == null) return NotFound();

        var viewModel = new InventoryApp.Models.ViewModels.InventoryDetailsViewModel
        {
            Inventory = inventory,
            ActiveTab = tab,
            IsOwnerOrAdmin = false,
            HasWriteAccess = false,
            Items = await _context.Items
                    .Where(i => i.InventoryId == id)
                    .OrderByDescending(i => i.Id)
                    .ToListAsync()
        };

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            viewModel.IsOwnerOrAdmin = (inventory.CreatorId == user.Id || User.IsInRole("Admin"));
            
            // Check write access
            viewModel.HasWriteAccess = viewModel.IsOwnerOrAdmin 
                                    || inventory.IsPublic 
                                    || inventory.Accesses.Any(a => a.UserId == user.Id);
        }

        if (viewModel.IsOwnerOrAdmin)
        {
            viewModel.CategorySelectList = new SelectList(_context.Categories, "Id", "Name", inventory.CategoryId);
            
            // Explicitly load Custom Id Parts for the Owner view
            await _context.Entry(inventory).Collection(i => i.CustomIdParts).LoadAsync();
        }

        if (tab == "statistics")
        {
            var stats = new InventoryStatisticsViewModel
            {
                TotalItems = viewModel.Items.Count()
            };

            var itemsList = viewModel.Items.ToList();

            if (itemsList.Any())
            {
                // Numeric Stats
                for (int i = 1; i <= 3; i++)
                {
                    bool isState = (bool)inventory.GetType().GetProperty("CustomInt" + i + "State")!.GetValue(inventory)!;
                    if (isState)
                    {
                        var name = inventory.GetType().GetProperty("CustomInt" + i + "Name")!.GetValue(inventory)?.ToString() ?? $"Number {i}";
                        var values = itemsList.Select(it => (int?)it.GetType().GetProperty("CustomInt" + i + "Value")!.GetValue(it))
                                              .Where(v => v.HasValue).Select(v => (double)v!.Value).ToList();
                        
                        if (values.Any())
                        {
                            stats.NumericStats[name] = (values.Min(), values.Max(), Math.Round(values.Average(), 2));
                        }
                    }
                }

                // String Stats (Top 3)
                for (int i = 1; i <= 3; i++)
                {
                    bool isState = (bool)inventory.GetType().GetProperty("CustomString" + i + "State")!.GetValue(inventory)!;
                    if (isState)
                    {
                        var name = inventory.GetType().GetProperty("CustomString" + i + "Name")!.GetValue(inventory)?.ToString() ?? $"String {i}";
                        var values = itemsList.Select(it => (string?)it.GetType().GetProperty("CustomString" + i + "Value")!.GetValue(it))
                                              .Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
                                              
                        if (values.Any())
                        {
                            var top3 = values.GroupBy(v => v!)
                                             .Select(g => (Value: g.Key, Count: g.Count()))
                                             .OrderByDescending(x => x.Count)
                                             .Take(3)
                                             .ToList();
                                             
                            stats.StringTopStats[name] = top3;
                        }
                    }
                }
            }

            viewModel.Statistics = stats;
        }

        return View(viewModel);
    }

    // GET: Inventories/Create
    [Authorize]
    public IActionResult Create()
    {
        ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
        return View();
    }

    // POST: Inventories/Create
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Inventory inventory)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();
        
        inventory.CreatorId = user.Id;

        // Clear navigation and auto-generated properties validation errors
        ModelState.Remove("Creator");
        ModelState.Remove("CreatorId");
        ModelState.Remove("Category");
        ModelState.Remove("InventoryTags");
        ModelState.Remove("Accesses");
        ModelState.Remove("CustomIdParts");
        ModelState.Remove("SearchVector");

        if (ModelState.IsValid)
        {
            _context.Add(inventory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", inventory.CategoryId);
        return View(inventory);
    }

    // GET: Inventories/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var inventory = await _context.Inventories.FindAsync(id);
        if (inventory == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null || (inventory.CreatorId != user.Id && !User.IsInRole("Admin")))
        {
            return Forbid();
        }

        ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", inventory.CategoryId);
        return View(inventory);
    }

    // POST: Inventories/Edit/5
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Inventory inventory)
    {
        if (id != inventory.Id) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null || (inventory.CreatorId != user.Id && !User.IsInRole("Admin")))
        {
            return Forbid();
        }

        ModelState.Remove("Creator");
        ModelState.Remove("Category");
        ModelState.Remove("InventoryTags");
        ModelState.Remove("Accesses");
        ModelState.Remove("CustomIdParts");
        ModelState.Remove("SearchVector");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(inventory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = inventory.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryExists(inventory.Id))
                {
                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError("Version", "Данные были изменены другим пользователем. Пожалуйста обновите страницу и примените изменения заново.");
                }
            }
        }
        
        ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", inventory.CategoryId);
        return View(inventory);
    }

    // POST: Inventories/UpdateSettings/5
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSettings(int id, Inventory inventorySettings)
    {
        if (id != inventorySettings.Id) return NotFound();

        var dbInventory = await _context.Inventories.FindAsync(id);
        if (dbInventory == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null || (dbInventory.CreatorId != user.Id && !User.IsInRole("Admin")))
        {
            return Forbid();
        }

        // Apply only Settings fields
        dbInventory.Title = inventorySettings.Title;
        dbInventory.Description = inventorySettings.Description;
        dbInventory.CategoryId = inventorySettings.CategoryId;
        dbInventory.ImageUrl = inventorySettings.ImageUrl;
        dbInventory.IsPublic = inventorySettings.IsPublic;

        // Important: check concurrency token
        _context.Entry(dbInventory).Property(i => i.Version).OriginalValue = inventorySettings.Version;

        try
        {
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = dbInventory.Id, tab = "settings" });
        }
        catch (DbUpdateConcurrencyException)
        {
             TempData["ErrorMessage"] = "Data changed by another user. Please refresh and try again.";
             return RedirectToAction(nameof(Details), new { id = dbInventory.Id, tab = "settings" });
        }
    }

    // POST: Inventories/UpdateFields/5
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateFields(int id, Inventory inventoryFields)
    {
        if (id != inventoryFields.Id) return NotFound();

        var dbInventory = await _context.Inventories.FindAsync(id);
        if (dbInventory == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null || (dbInventory.CreatorId != user.Id && !User.IsInRole("Admin")))
        {
            return Forbid();
        }

        // Apply only Fields mapping
        for(int i=1; i<=3; i++)
        {
            var stringState = (bool)inventoryFields.GetType().GetProperty("CustomString"+i+"State").GetValue(inventoryFields);
            var stringName = inventoryFields.GetType().GetProperty("CustomString"+i+"Name").GetValue(inventoryFields)?.ToString();
            dbInventory.GetType().GetProperty("CustomString"+i+"State").SetValue(dbInventory, stringState);
            dbInventory.GetType().GetProperty("CustomString"+i+"Name").SetValue(dbInventory, stringName);

            var textState = (bool)inventoryFields.GetType().GetProperty("CustomText"+i+"State").GetValue(inventoryFields);
            var textName = inventoryFields.GetType().GetProperty("CustomText"+i+"Name").GetValue(inventoryFields)?.ToString();
            dbInventory.GetType().GetProperty("CustomText"+i+"State").SetValue(dbInventory, textState);
            dbInventory.GetType().GetProperty("CustomText"+i+"Name").SetValue(dbInventory, textName);

            var intState = (bool)inventoryFields.GetType().GetProperty("CustomInt"+i+"State").GetValue(inventoryFields);
            var intName = inventoryFields.GetType().GetProperty("CustomInt"+i+"Name").GetValue(inventoryFields)?.ToString();
            dbInventory.GetType().GetProperty("CustomInt"+i+"State").SetValue(dbInventory, intState);
            dbInventory.GetType().GetProperty("CustomInt"+i+"Name").SetValue(dbInventory, intName);

            var boolState = (bool)inventoryFields.GetType().GetProperty("CustomBool"+i+"State").GetValue(inventoryFields);
            var boolName = inventoryFields.GetType().GetProperty("CustomBool"+i+"Name").GetValue(inventoryFields)?.ToString();
            dbInventory.GetType().GetProperty("CustomBool"+i+"State").SetValue(dbInventory, boolState);
            dbInventory.GetType().GetProperty("CustomBool"+i+"Name").SetValue(dbInventory, boolName);

            var linkState = (bool)inventoryFields.GetType().GetProperty("CustomLink"+i+"State").GetValue(inventoryFields);
            var linkName = inventoryFields.GetType().GetProperty("CustomLink"+i+"Name").GetValue(inventoryFields)?.ToString();
            dbInventory.GetType().GetProperty("CustomLink"+i+"State").SetValue(dbInventory, linkState);
            dbInventory.GetType().GetProperty("CustomLink"+i+"Name").SetValue(dbInventory, linkName);
        }

        // Set optimistic lock tracker
        _context.Entry(dbInventory).Property(i => i.Version).OriginalValue = inventoryFields.Version;

        try
        {
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = dbInventory.Id, tab = "fields" });
        }
        catch (DbUpdateConcurrencyException)
        {
             TempData["ErrorMessage"] = "Data changed by another user. Please refresh and try again.";
             return RedirectToAction(nameof(Details), new { id = dbInventory.Id, tab = "fields" });
        }
    }

    // POST: Inventories/UpdateCustomIdMapping/5
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCustomIdMapping(int id, List<CustomIdPart> parts, int defaultVersion)
    {
        var dbInventory = await _context.Inventories
            .Include(i => i.CustomIdParts)
            .FirstOrDefaultAsync(i => i.Id == id);
            
        if (dbInventory == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null || (dbInventory.CreatorId != user.Id && !User.IsInRole("Admin")))
        {
            return Forbid();
        }

        // Clear existing parts and apply new ones
        _context.CustomIdParts.RemoveRange(dbInventory.CustomIdParts);
        
        int order = 0;
        foreach(var part in parts.Where(p => !string.IsNullOrEmpty(p.PartType)))
        {
            part.InventoryId = dbInventory.Id;
            part.Order = order++;
            
            // Cleanup irrelevant fields based on type
            if (part.PartType != "FixedText") part.TextValue = null;
            if (part.PartType != "DateTime") part.DateFormat = null;
            if (part.PartType != "Sequence") part.Padding = null;

            _context.CustomIdParts.Add(part);
        }

        // Optimistic locking via Inventory Version
        _context.Entry(dbInventory).Property(i => i.Version).OriginalValue = defaultVersion;

        try
        {
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = dbInventory.Id, tab = "customid" });
        }
        catch (DbUpdateConcurrencyException)
        {
             TempData["ErrorMessage"] = "Data changed by another user. Please refresh and try again.";
             return RedirectToAction(nameof(Details), new { id = dbInventory.Id, tab = "customid" });
        }
    }

    // GET: Inventories/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var inventory = await _context.Inventories
            .Include(i => i.Category)
            .Include(i => i.Creator)
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (inventory == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null || (inventory.CreatorId != user.Id && !User.IsInRole("Admin")))
        {
            return Forbid();
        }

        return View(inventory);
    }

    // POST: Inventories/Delete/5
    [HttpPost, ActionName("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var inventory = await _context.Inventories.FindAsync(id);
        if (inventory == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null || (inventory.CreatorId != user.Id && !User.IsInRole("Admin")))
        {
            return Forbid();
        }

        _context.Inventories.Remove(inventory);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST: Inventories/GrantAccess
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GrantAccess(int inventoryId, string userEmailOrName)
    {
        var dbInventory = await _context.Inventories.FindAsync(inventoryId);
        if (dbInventory == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null || (dbInventory.CreatorId != currentUser.Id && !User.IsInRole("Admin")))
        {
            return Forbid();
        }

        var targetUser = await _userManager.FindByEmailAsync(userEmailOrName) 
                         ?? await _userManager.FindByNameAsync(userEmailOrName);

        if (targetUser != null && targetUser.Id != dbInventory.CreatorId)
        {
            var exists = await _context.InventoryAccesses.AnyAsync(a => a.InventoryId == inventoryId && a.UserId == targetUser.Id);
            if (!exists)
            {
                _context.InventoryAccesses.Add(new InventoryAccess { InventoryId = inventoryId, UserId = targetUser.Id });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Access granted to {targetUser.UserName}.";
            }
        }
        else
        {
            TempData["ErrorMessage"] = "User not found or is already the owner.";
        }

        return RedirectToAction(nameof(Details), new { id = inventoryId, tab = "access" });
    }

    // POST: Inventories/RevokeAccess
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeAccess(int inventoryId, List<string> userIds)
    {
        var dbInventory = await _context.Inventories.FindAsync(inventoryId);
        if (dbInventory == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null || (dbInventory.CreatorId != currentUser.Id && !User.IsInRole("Admin")))
        {
            return Forbid();
        }

        if (userIds != null && userIds.Any())
        {
            var accesses = await _context.InventoryAccesses
                .Where(a => a.InventoryId == inventoryId && userIds.Contains(a.UserId))
                .ToListAsync();

            if (accesses.Any())
            {
                _context.InventoryAccesses.RemoveRange(accesses);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Selected users have been removed from access list.";
            }
        }

        return RedirectToAction(nameof(Details), new { id = inventoryId, tab = "access" });
    }

    private bool InventoryExists(int id)
    {
        return _context.Inventories.Any(e => e.Id == id);
    }
}
