using Cashflow.Web.Models.Enums;

namespace Cashflow.Web.Models.Entities;

public class PaymentRequest
{
    public long Id { get; set; }

    public int CompanyId { get; set; }

    public CompanyMaster Company { get; set; } = null!;

    public int VendorMasterId { get; set; }

    public VendorMaster VendorMaster { get; set; } = null!;

    public decimal RequestedAmount { get; set; }

    public PaymentRequestStatus Status { get; set; } = PaymentRequestStatus.Pending;

    public int? ApprovedLedgerMasterId { get; set; }

    public LedgerMaster? ApprovedLedgerMaster { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public string? RequestNotes { get; set; }

    public string? ReviewNotes { get; set; }

    public string RequestedByUserId { get; set; } = string.Empty;

    public ApplicationUser RequestedByUser { get; set; } = null!;

    public string? ReviewedByUserId { get; set; }

    public ApplicationUser? ReviewedByUser { get; set; }

    public long? ParentPaymentRequestId { get; set; }

    public PaymentRequest? ParentPaymentRequest { get; set; }

    public ICollection<PaymentRequest> SplitChildren { get; set; } = new List<PaymentRequest>();

    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAtUtc { get; set; }

    public DateTime? ScheduledPaymentDateUtc { get; set; }
}