using System.ComponentModel.DataAnnotations;

namespace Cashflow.Web.ViewModels.UserManagement;

public class SubsidiaryUserCreateViewModel : SubsidiaryUserFormBaseViewModel
{
    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}