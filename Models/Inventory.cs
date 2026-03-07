using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace InventoryApp.Models;

public class Inventory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(2048)]
    public string? ImageUrl { get; set; }

    [Required]
    public string CreatorId { get; set; } = string.Empty;
    public IdentityUser? Creator { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    // If IsPublic is true, any authenticated user can write ITEMS to this inventory.
    // Read access is global anyway.
    public bool IsPublic { get; set; } = false;

    [ConcurrencyCheck]
    public int Version { get; set; }

    // --- Custom Field Definitions (Metadata) ---

    [MaxLength(100)] public string? CustomString1Name { get; set; }
    public bool CustomString1State { get; set; }
    [MaxLength(100)] public string? CustomString2Name { get; set; }
    public bool CustomString2State { get; set; }
    [MaxLength(100)] public string? CustomString3Name { get; set; }
    public bool CustomString3State { get; set; }

    [MaxLength(100)] public string? CustomText1Name { get; set; }
    public bool CustomText1State { get; set; }
    [MaxLength(100)] public string? CustomText2Name { get; set; }
    public bool CustomText2State { get; set; }
    [MaxLength(100)] public string? CustomText3Name { get; set; }
    public bool CustomText3State { get; set; }

    [MaxLength(100)] public string? CustomInt1Name { get; set; }
    public bool CustomInt1State { get; set; }
    [MaxLength(100)] public string? CustomInt2Name { get; set; }
    public bool CustomInt2State { get; set; }
    [MaxLength(100)] public string? CustomInt3Name { get; set; }
    public bool CustomInt3State { get; set; }

    [MaxLength(100)] public string? CustomBool1Name { get; set; }
    public bool CustomBool1State { get; set; }
    [MaxLength(100)] public string? CustomBool2Name { get; set; }
    public bool CustomBool2State { get; set; }
    [MaxLength(100)] public string? CustomBool3Name { get; set; }
    public bool CustomBool3State { get; set; }

    [MaxLength(100)] public string? CustomLink1Name { get; set; }
    public bool CustomLink1State { get; set; }
    [MaxLength(100)] public string? CustomLink2Name { get; set; }
    public bool CustomLink2State { get; set; }
    [MaxLength(100)] public string? CustomLink3Name { get; set; }
    public bool CustomLink3State { get; set; }

    public ICollection<InventoryTag> InventoryTags { get; set; } = new List<InventoryTag>();
    public ICollection<InventoryAccess> Accesses { get; set; } = new List<InventoryAccess>();

    // --- Custom ID Formatting ---
    // Start at 1, incremented automatically when an Item is created with a 'Sequence' part.
    public int NextSequenceValue { get; set; } = 1;
    public ICollection<CustomIdPart> CustomIdParts { get; set; } = new List<CustomIdPart>();
}
