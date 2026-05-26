using Cashflow.Web.Models.Enums;

namespace Cashflow.Web.Models.Entities;

public class LedgerEntry
{
    public long Id { get; set; }

    public int LedgerMasterId { get; set; }

    public LedgerMaster LedgerMaster { get; set; } = null!;

    public DateOnly EntryDate { get; set; }

    public LedgerEntryType EntryType { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public long? PaymentRequestId { get; set; }

    public PaymentRequest? PaymentRequest { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;

    public ApplicationUser CreatedByUser { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}