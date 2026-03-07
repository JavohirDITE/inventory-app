using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using InventoryApp.Models;

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
            HasWriteAccess = false
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

        // Clear Creator/Category navigation properties validation errors if any because they are objects
        ModelState.Remove("Creator");
        ModelState.Remove("Category");

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

    private bool InventoryExists(int id)
    {
        return _context.Inventories.Any(e => e.Id == id);
    }
}
