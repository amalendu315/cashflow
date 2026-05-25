namespace Cashflow.Web.ViewModels.UserManagement;

public class SubsidiaryUserListItemViewModel
{
    public string Id { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string CompanyCode { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}