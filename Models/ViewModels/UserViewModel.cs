namespace InventoryApp.Models.ViewModels;

public class UserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsBlocked { get; set; }
}
