using InventoryApp.Models;

namespace InventoryApp.Models.ViewModels;

public class SearchViewModel
{
    public string Query { get; set; } = string.Empty;
    public string NormalizedQuery { get; set; } = string.Empty;

    public List<Inventory> Inventories { get; set; } = new List<Inventory>();
    public List<Item> Items { get; set; } = new List<Item>();

    public bool HasResults => Inventories.Any() || Items.Any();
}
