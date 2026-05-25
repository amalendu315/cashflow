using Cashflow.Web.Models.Enums;
using Cashflow.Web.Utilities;
using System.Globalization;

namespace Cashflow.Web.ViewModels.PaymentRequests;

public class PaymentRequestListItemViewModel
{
    public long Id { get; set; }

    public string VendorCode { get; set; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;

    public decimal RequestedAmount { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public PaymentRequestStatus Status { get; set; }

    public PaymentPriority Priority { get; set; }

    public DateOnly ScheduledPaymentDate { get; set; }

    public string? RequestNotes { get; set; }

    public string? ReviewNotes { get; set; }

    public string RequestedByName { get; set; } = string.Empty;

    public string RequestedByEmail { get; set; } = string.Empty;

    public DateTime RequestedAtUtc { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }

    public long? ParentPaymentRequestId { get; set; }

    public string RequestIdDisplay => $"PR-{Id:D6}";

    public string RequestedAmountDisplay =>
        "₹" + RequestedAmount.ToString("N2", CultureInfo.InvariantCulture);

    public string ApprovedAmountDisplay =>
        ApprovedAmount.HasValue
            ? "₹" + ApprovedAmount.Value.ToString("N2", CultureInfo.InvariantCulture)
            : "—";

    public string ScheduledPaymentDateDisplay =>
        ScheduledPaymentDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + " IST";

    public string RequestedAtDisplay =>
        AppClock.ConvertUtcToIst(RequestedAtUtc)
            .ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) + " IST";

    public string ReviewedAtDisplay =>
        ReviewedAtUtc.HasValue
            ? AppClock.ConvertUtcToIst(ReviewedAtUtc.Value)
                .ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) + " IST"
            : "—";

    public string RequestNotesDisplay =>
        string.IsNullOrWhiteSpace(RequestNotes) ? "—" : RequestNotes;

    public string ReviewNotesDisplay =>
        string.IsNullOrWhiteSpace(ReviewNotes) ? "—" : ReviewNotes;

    public string StatusBadgeClass => Status switch
    {
        PaymentRequestStatus.Pending => "text-bg-warning",
        PaymentRequestStatus.Approved => "text-bg-success",
        PaymentRequestStatus.Split => "text-bg-info",
        PaymentRequestStatus.Rejected => "text-bg-danger",
        _ => "text-bg-secondary"
    };

    public string PriorityBadgeClass => Priority switch
    {
        PaymentPriority.High => "text-bg-danger",
        PaymentPriority.Medium => "text-bg-warning",
        PaymentPriority.Normal => "text-bg-secondary",
        _ => "text-bg-secondary"
    };
}