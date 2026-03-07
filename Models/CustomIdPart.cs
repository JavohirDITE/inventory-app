using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models;

public class CustomIdPart
{
    public int Id { get; set; }

    public int InventoryId { get; set; }
    public Inventory? Inventory { get; set; }

    public int Order { get; set; }

    // E.g., "FixedText", "RandomHex20", "RandomHex32", "RandomDigits6", "RandomDigits9", "Guid", "DateTime", "Sequence"
    [Required]
    [MaxLength(50)]
    public string PartType { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TextValue { get; set; }

    [MaxLength(50)]
    public string? DateFormat { get; set; }

    public int? Padding { get; set; }
}
