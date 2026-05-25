namespace Cashflow.Web.ViewModels.VendorMasters;

public class VendorMasterListItemViewModel
{
    public int Id { get; set; }

    public string VendorCode { get; set; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;

    public string? GstNumber { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int PaymentRequestCount { get; set; }
}