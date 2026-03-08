using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;

namespace InventoryApp.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;

    public UsersController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Ok(new List<object>());

        var query = q.ToLower();
        var users = await _userManager.Users
            .Where(u => u.UserName!.ToLower().Contains(query) || u.Email!.ToLower().Contains(query))
            .Take(10)
            .Select(u => new { id = u.Id, userName = u.UserName, email = u.Email })
            .ToListAsync();

        return Ok(users);
    }
}
