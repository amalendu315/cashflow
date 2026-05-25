using System.ComponentModel.DataAnnotations;

namespace Cashflow.Web.ViewModels.VendorMasters;

public class VendorMasterFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Vendor Code")]
    public string VendorCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    [Display(Name = "Vendor Name")]
    public string VendorName { get; set; } = string.Empty;

    [StringLength(30)]
    [Display(Name = "GST Number")]
    public string? GstNumber { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}