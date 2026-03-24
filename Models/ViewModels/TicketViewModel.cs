using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models.ViewModels;

public class TicketViewModel
{
    [Required(ErrorMessage = "Summary is required")]
    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    public string Priority { get; set; } = "Average";

    public string? InventoryTitle { get; set; }
    public string? PageLink { get; set; }
}
