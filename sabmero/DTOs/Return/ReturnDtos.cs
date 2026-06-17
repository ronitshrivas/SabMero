using System.ComponentModel.DataAnnotations;

namespace sabmero.DTOs.Return;

// ── Customer SENDS this to request a return/refund on a delivered order ──
public class CreateReturnDto
{
    [Required]
    public int OrderId { get; set; }

    [Required(ErrorMessage = "A reason is required")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

// ── Admin SENDS this to approve or reject a return ──
public class ResolveReturnDto
{
    [Required]
    // "Approved" | "Rejected"
    public string Status { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? AdminNote { get; set; }
}

// ── What the API SENDS BACK ──
public class ReturnDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal OrderTotal { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; }
}