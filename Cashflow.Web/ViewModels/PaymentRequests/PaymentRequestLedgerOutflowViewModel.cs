using Cashflow.Web.Utilities;
using System.Globalization;

namespace Cashflow.Web.ViewModels.PaymentRequests;

public class PaymentRequestLedgerOutflowViewModel
{
    public long LedgerEntryId { get; set; }

    public string LedgerCode { get; set; } = string.Empty;

    public string LedgerName { get; set; } = string.Empty;

    public DateOnly EntryDate { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public string CreatedByName { get; set; } = string.Empty;

    public string CreatedByEmail { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public string LedgerDisplay => $"{LedgerName} ({LedgerCode})";

    public string EntryDateDisplay =>
        EntryDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + " IST";

    public string AmountDisplay =>
        "₹" + Amount.ToString("N2", CultureInfo.InvariantCulture);

    public string DescriptionDisplay =>
        string.IsNullOrWhiteSpace(Description) ? "—" : Description;

    public string CreatedAtDisplay =>
        AppClock.ConvertUtcToIst(CreatedAtUtc)
            .ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) + " IST";
}