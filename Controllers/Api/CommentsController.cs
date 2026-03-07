using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using InventoryApp.Data;
using InventoryApp.Models;
using InventoryApp.Hubs;

namespace InventoryApp.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHubContext<DiscussionHub> _hubContext;

    public CommentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IHubContext<DiscussionHub> hubContext)
    {
        _context = context;
        _userManager = userManager;
        _hubContext = hubContext;
    }

    [HttpGet("{inventoryId}")]
    public async Task<IActionResult> GetComments(int inventoryId)
    {
        var comments = await _context.Comments
            .Where(c => c.InventoryId == inventoryId)
            .Include(c => c.User)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new {
                id = c.Id,
                content = c.Content,
                userName = c.User != null ? c.User.UserName : "Unknown",
                userId = c.UserId,
                createdAt = c.CreatedAt.ToString("o")
            })
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PostComment([FromBody] CommentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest();

        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();

        var comment = new Comment
        {
            InventoryId = request.InventoryId,
            UserId = userId,
            Content = request.Content
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var userName = User.Identity?.Name ?? "Unknown";
        var response = new {
            id = comment.Id,
            content = comment.Content,
            userName = userName,
            userId = comment.UserId,
            createdAt = comment.CreatedAt.ToString("o")
        };

        await _hubContext.Clients.Group(request.InventoryId.ToString()).SendAsync("ReceiveComment", response);

        return Ok(response);
    }
}

public class CommentRequest
{
    public int InventoryId { get; set; }
    public string Content { get; set; } = string.Empty;
}
