using System.ComponentModel.DataAnnotations;

namespace Cashflow.Web.ViewModels.CompanyMasters;

public class CompanyMasterFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Company Code")]
    public string CompanyCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    [Display(Name = "Company Name")]
    public string CompanyName { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}