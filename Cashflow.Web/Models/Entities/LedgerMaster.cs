namespace Cashflow.Web.Models.Entities;

public class LedgerMaster
{
    public int Id { get; set; }

    public string LedgerCode { get; set; } = string.Empty;

    public string LedgerName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<PaymentRequest> ApprovedPaymentRequests { get; set; } = new List<PaymentRequest>();
}