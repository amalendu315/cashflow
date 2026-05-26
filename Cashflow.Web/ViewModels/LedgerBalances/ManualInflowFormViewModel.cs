using System.ComponentModel.DataAnnotations;

namespace Cashflow.Web.ViewModels.LedgerBalances;

public class ManualInflowFormViewModel
{
    [Required]
    public int? LedgerMasterId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateOnly? EntryDate { get; set; }

    [Required]
    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ErrorMessage = "Inflow amount must be greater than zero.")]
    [Display(Name = "Inflow Amount")]
    public decimal? Amount { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
}