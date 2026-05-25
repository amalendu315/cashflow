using System.ComponentModel.DataAnnotations;

namespace Cashflow.Web.ViewModels.LedgerMasters;

public class LedgerMasterFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Ledger Code")]
    public string LedgerCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    [Display(Name = "Ledger Name")]
    public string LedgerName { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}