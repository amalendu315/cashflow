namespace Cashflow.Web.ViewModels.LedgerMasters;

public class LedgerMasterListItemViewModel
{
    public int Id { get; set; }

    public string LedgerCode { get; set; } = string.Empty;

    public string LedgerName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int ApprovedPaymentRequestCount { get; set; }
}