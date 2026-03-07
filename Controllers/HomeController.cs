using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Models;
using InventoryApp.Data;

namespace InventoryApp.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        bool dbConnected = false;
        try
        {
            dbConnected = _context.Database.CanConnect();
        }
        catch (Exception)
        {
            dbConnected = false;
        }
        ViewBag.DbConnected = dbConnected;

        if (dbConnected)
        {
            var recentInventories = await _context.Inventories
                .Include(i => i.Category)
                .Include(i => i.Creator)
                .OrderByDescending(i => i.Id)
                .Take(5)
                .ToListAsync();
            
            return View(recentInventories);
        }

        return View(new List<Inventory>());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
