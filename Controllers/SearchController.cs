using Microsoft.AspNetCore.Mvc;
using InventoryApp.Data;
using InventoryApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Controllers;

public class SearchController : Controller
{
    private readonly ApplicationDbContext _context;

    public SearchController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var vm = new SearchViewModel { Query = q ?? string.Empty };
        
        if (string.IsNullOrWhiteSpace(q))
        {
            return View(vm);
        }

        // 1. Safe Query Normalization
        // Keep alphanumeric and spaces, discard punctuation that breaks tsquery
        var safeChars = q.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray();
        var safeQuery = new string(safeChars).Trim();

        var words = safeQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return View(vm);

        // AND logic for multi-word search: "wooden & chair:*"
        var tsQueryString = string.Join(" & ", words) + ":*"; 
        vm.NormalizedQuery = tsQueryString;

        // 2. Perform FTS on Inventories (limit 50)
        vm.Inventories = await _context.Inventories
            .Include(i => i.Category)
            .Include(i => i.Creator)
            .Where(i => i.SearchVector.Matches(EF.Functions.ToTsQuery("simple", tsQueryString)))
            // Rank by relevance
            .OrderByDescending(i => i.SearchVector.Rank(EF.Functions.ToTsQuery("simple", tsQueryString)))
            .Take(50)
            .AsNoTracking()
            .ToListAsync();

        // 3. Perform FTS on Items (limit 150)
        vm.Items = await _context.Items
            .Include(i => i.Inventory)
            .Where(i => i.SearchVector.Matches(EF.Functions.ToTsQuery("simple", tsQueryString)))
            .OrderByDescending(i => i.SearchVector.Rank(EF.Functions.ToTsQuery("simple", tsQueryString)))
            .Take(150)
            .AsNoTracking()
            .ToListAsync();

        return View(vm);
    }
}
