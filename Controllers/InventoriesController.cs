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
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var inventory = await _context.Inventories
            .Include(i => i.Category)
            .Include(i => i.Creator)
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (inventory == null) return NotFound();

        return View(inventory);
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
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryExists(inventory.Id))
                {
                    return NotFound();
                }
                else
                {
                    // Оптимистическая блокировка - кто-то другой уже сохранил данные
                    ModelState.AddModelError("Version", "Данные были изменены другим пользователем. Пожалуйста обновите страницу и примените изменения заново.");
                    
                    // Нужно перезагрузить модель из БД или просто сообщить об ошибке
                }
            }
        }
        
        ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", inventory.CategoryId);
        return View(inventory);
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
