using InventoryApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryApp.Models.ViewModels;

public class InventoryDetailsViewModel
{
    public Inventory Inventory { get; set; } = null!;
    
    // Whether the current user is the Creator or an Admin
    public bool IsOwnerOrAdmin { get; set; }
    
    // Whether the current user has write access (can add items, post comments, etc)
    // Write access is true if: IsOwnerOrAdmin OR Inventory.IsPublic OR UserId in InventoryAccess
    public bool HasWriteAccess { get; set; }
    
    // Dropdown list data for the Settings Tab category editor
    public SelectList? CategorySelectList { get; set; }
    
    // Keep track of which tab to show active on page load/re-load
    public string ActiveTab { get; set; } = "items";

    // The items belonging to this inventory
    public IEnumerable<Item> Items { get; set; } = new List<Item>();
    
    // Statistics for the inventory
    public InventoryStatisticsViewModel? Statistics { get; set; }
}

public class InventoryStatisticsViewModel
{
    public int TotalItems { get; set; }
    
    // Key = Custom Field Name, Value = Tuple(Min, Max, Avg)
    public Dictionary<string, (double Min, double Max, double Avg)> NumericStats { get; set; } = new();

    // Key = Custom Field Name, Value = List of (Value, Count)
    public Dictionary<string, List<(string Value, int Count)>> StringTopStats { get; set; } = new();
}
