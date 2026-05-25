using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Cashflow.Web.ViewModels.UserManagement;

public abstract class SubsidiaryUserFormBaseViewModel
{
    [Required]
    [StringLength(150)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Company")]
    public int? CompanyId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<SelectListItem> CompanyOptions { get; set; }
        = new List<SelectListItem>();
}