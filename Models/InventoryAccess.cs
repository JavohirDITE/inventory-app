using Microsoft.AspNetCore.Identity;

namespace InventoryApp.Models;

public class InventoryAccess
{
    public int InventoryId { get; set; }
    public Inventory? Inventory { get; set; }

    // Using string for UserId because IdentityUser uses string Id by default
    public string UserId { get; set; } = string.Empty;
    public IdentityUser? User { get; set; }
}
