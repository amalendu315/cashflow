using Cashflow.Web.Models.Enums;
using Cashflow.Web.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Cashflow.Web.ViewModels.AdminPaymentRequests;

public class AdminPaymentRequestReviewViewModel
{
    public long Id { get; set; }

    public string CompanyCode { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string VendorCode { get; set; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;

    public decimal RequestedAmount { get; set; }

    public PaymentPriority Priority { get; set; }

    public DateOnly ScheduledPaymentDate { get; set; }

    public string? RequestNotes { get; set; }

    public string RequestedByName { get; set; } = string.Empty;

    public string RequestedByEmail { get; set; } = string.Empty;

    public DateTime RequestedAtUtc { get; set; }

    [Display(Name = "Ledger")]
    public int? ApprovedLedgerMasterId { get; set; }

    [StringLength(500)]
    [Display(Name = "Review Notes")]
    public string? ReviewNotes { get; set; }

    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ErrorMessage = "Approved amount must be greater than zero.")]
    [Display(Name = "Approved Amount")]
    public decimal? SplitApprovedAmount { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Remainder Scheduled Payment Date")]
    public DateOnly? SplitRemainderScheduledPaymentDate { get; set; } = AppClock.TodayIst();

    public IReadOnlyList<SelectListItem> LedgerOptions { get; set; }
        = new List<SelectListItem>();

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