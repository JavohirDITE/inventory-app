using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Controllers;

public class SearchController : Controller
{
    public IActionResult Index(string q)
    {
        ViewBag.Query = q;
        // In Phase 6 this will actually perform full text search via PostgreSQL tsvector
        return View();
    }
}
