using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models;

public class Tag
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<InventoryTag> InventoryTags { get; set; } = new List<InventoryTag>();
}
