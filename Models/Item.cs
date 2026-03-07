using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace InventoryApp.Models;

public class Item
{
    public int Id { get; set; }

    public int InventoryId { get; set; }
    public Inventory? Inventory { get; set; }

    [Required]
    [MaxLength(100)]
    public string CustomId { get; set; } = string.Empty;

    public string CreatedById { get; set; } = string.Empty;
    public IdentityUser? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ConcurrencyCheck]
    public int Version { get; set; }

    public NpgsqlTypes.NpgsqlTsVector? SearchVector { get; set; }

    // --- Fixed 15 Slots for Custom Field Data ---
    [MaxLength(255)] public string? String1 { get; set; }
    [MaxLength(255)] public string? String2 { get; set; }
    [MaxLength(255)] public string? String3 { get; set; }

    public string? Text1 { get; set; }
    public string? Text2 { get; set; }
    public string? Text3 { get; set; }

    public int? Int1 { get; set; }
    public int? Int2 { get; set; }
    public int? Int3 { get; set; }

    public bool? Bool1 { get; set; }
    public bool? Bool2 { get; set; }
    public bool? Bool3 { get; set; }

    [MaxLength(2048)] public string? Link1 { get; set; }
    [MaxLength(2048)] public string? Link2 { get; set; }
    [MaxLength(2048)] public string? Link3 { get; set; }
}
