using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Data;
using InventoryApp.Models;

namespace InventoryApp.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class LikesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public LikesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost("toggle/{itemId}")]
    [Authorize]
    public async Task<IActionResult> ToggleLike(int itemId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();

        var existingLike = await _context.Likes.FirstOrDefaultAsync(l => l.ItemId == itemId && l.UserId == userId);
        bool liked = false;

        if (existingLike != null)
        {
            _context.Likes.Remove(existingLike);
        }
        else
        {
            _context.Likes.Add(new Like { ItemId = itemId, UserId = userId });
            liked = true;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Handle race condition of double-clicks causing unique constraint violation
            return BadRequest(new { error = "Optimistic concurrency or unique constraint violation." });
        }

        var newCount = await _context.Likes.CountAsync(l => l.ItemId == itemId);
        
        return Ok(new { liked = liked, count = newCount });
    }

    [HttpGet("count/{itemId}")]
    public async Task<IActionResult> GetCount(int itemId)
    {
        var count = await _context.Likes.CountAsync(l => l.ItemId == itemId);
        var liked = false;

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            liked = await _context.Likes.AnyAsync(l => l.ItemId == itemId && l.UserId == userId);
        }

        return Ok(new { liked, count });
    }
}
