namespace Cashflow.Web.ViewModels.CompanyMasters;

public class CompanyMasterListItemViewModel
{
    public int Id { get; set; }

    public string CompanyCode { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int UserCount { get; set; }

    public int PaymentRequestCount { get; set; }
}