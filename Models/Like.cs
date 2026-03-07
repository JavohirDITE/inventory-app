using Microsoft.AspNetCore.Identity;

namespace InventoryApp.Models;

public class Like
{
    public int Id { get; set; }

    public int ItemId { get; set; }
    public Item? Item { get; set; }

    public string UserId { get; set; } = string.Empty;
    public IdentityUser? User { get; set; }
}
