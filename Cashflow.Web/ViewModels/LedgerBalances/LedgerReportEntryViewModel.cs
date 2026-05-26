using Cashflow.Web.Models.Enums;
using Cashflow.Web.Utilities;
using System.Globalization;

namespace Cashflow.Web.ViewModels.LedgerBalances;

public class LedgerReportEntryViewModel
{
    public long Id { get; set; }

    public DateOnly EntryDate { get; set; }

    public LedgerEntryType EntryType { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public long? PaymentRequestId { get; set; }

    public string CreatedByName { get; set; } = string.Empty;

    public string CreatedByEmail { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public string EntryDateDisplay =>
        EntryDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + " IST";

    public string AmountDisplay =>
        "₹" + Amount.ToString("N2", CultureInfo.InvariantCulture);

    public string DescriptionDisplay =>
        string.IsNullOrWhiteSpace(Description) ? "—" : Description;

    public string PaymentRequestDisplay =>
        PaymentRequestId.HasValue ? $"PR-{PaymentRequestId.Value:D6}" : "—";

    public string CreatedAtDisplay =>
        AppClock.ConvertUtcToIst(CreatedAtUtc)
            .ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) + " IST";

    public string EntryTypeBadgeClass => EntryType switch
    {
        LedgerEntryType.OpeningBalance => "text-bg-info",
        LedgerEntryType.Inflow => "text-bg-success",
        LedgerEntryType.Outflow => "text-bg-danger",
        _ => "text-bg-secondary"
    };
}