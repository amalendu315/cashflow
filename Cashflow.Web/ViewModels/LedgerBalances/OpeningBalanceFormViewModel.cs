using System.ComponentModel.DataAnnotations;

namespace Cashflow.Web.ViewModels.LedgerBalances;

public class OpeningBalanceFormViewModel
{
    [Required]
    public int? LedgerMasterId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateOnly? EntryDate { get; set; }

    [Required]
    [Range(
        typeof(decimal),
        "0",
        "9999999999999999.99",
        ErrorMessage = "Opening balance cannot be negative.")]
    [Display(Name = "Opening Balance")]
    public decimal? Amount { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}