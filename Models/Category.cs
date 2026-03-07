using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
