using Cashflow.Web.Models.Enums;
using Cashflow.Web.Utilities;
using System.Globalization;

namespace Cashflow.Web.ViewModels.AdminPaymentRequests;

public class AdminPaymentRequestListItemViewModel
{
    public long Id { get; set; }

    public string CompanyCode { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string VendorCode { get; set; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;

    public decimal RequestedAmount { get; set; }

    public PaymentPriority Priority { get; set; }

    public DateOnly ScheduledPaymentDate { get; set; }

    public string RequestedByName { get; set; } = string.Empty;

    public string RequestedByEmail { get; set; } = string.Empty;

    public DateTime RequestedAtUtc { get; set; }

    public string? RequestNotes { get; set; }

    public string RequestIdDisplay => $"PR-{Id:D6}";

    public string RequestedAmountDisplay =>
        "₹" + RequestedAmount.ToString("N2", CultureInfo.InvariantCulture);

    public string ScheduledPaymentDateDisplay =>
        ScheduledPaymentDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + " IST";

    public string RequestedAtDisplay =>
        AppClock.ConvertUtcToIst(RequestedAtUtc)
            .ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) + " IST";

    public string CompanyDisplay => $"{CompanyName} ({CompanyCode})";

    public string VendorDisplay => $"{VendorName} ({VendorCode})";

    public string RequestNotesDisplay =>
        string.IsNullOrWhiteSpace(RequestNotes) ? "—" : RequestNotes;

    public string PriorityBadgeClass => Priority switch
    {
        PaymentPriority.High => "text-bg-danger",
        PaymentPriority.Medium => "text-bg-warning",
        PaymentPriority.Normal => "text-bg-secondary",
        _ => "text-bg-secondary"
    };
}