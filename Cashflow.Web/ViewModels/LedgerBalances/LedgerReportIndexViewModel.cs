using Cashflow.Web.Models.Enums;
using Cashflow.Web.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Cashflow.Web.ViewModels.LedgerBalances;

public class LedgerReportIndexViewModel
{
    [Required]
    [Display(Name = "Ledger")]
    public int? LedgerMasterId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateOnly FromDate { get; set; } = AppClock.TodayIst();

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateOnly ToDate { get; set; } = AppClock.TodayIst();

    public string LedgerCode { get; set; } = string.Empty;

    public string LedgerName { get; set; } = string.Empty;

    public IReadOnlyList<SelectListItem> LedgerOptions { get; set; }
        = new List<SelectListItem>();

    public IReadOnlyList<LedgerReportEntryViewModel> Entries { get; set; }
        = new List<LedgerReportEntryViewModel>();

    public bool HasLedgerSelected => LedgerMasterId.HasValue;

    public bool HasEntries => Entries.Any();

    public bool HasDateError => FromDate > ToDate;

    public string LedgerDisplay =>
        string.IsNullOrWhiteSpace(LedgerCode)
            ? LedgerName
            : $"{LedgerName} ({LedgerCode})";

    public decimal OpeningBalanceTotal =>
        Entries
            .Where(entry => entry.EntryType == LedgerEntryType.OpeningBalance)
            .Sum(entry => entry.Amount);

    public decimal InflowTotal =>
        Entries
            .Where(entry => entry.EntryType == LedgerEntryType.Inflow)
            .Sum(entry => entry.Amount);

    public decimal OutflowTotal =>
        Entries
            .Where(entry => entry.EntryType == LedgerEntryType.Outflow)
            .Sum(entry => entry.Amount);

    public decimal NetMovement => InflowTotal - OutflowTotal;

    public decimal RangeBalanceEffect => OpeningBalanceTotal + InflowTotal - OutflowTotal;

    public string OpeningBalanceTotalDisplay =>
        "₹" + OpeningBalanceTotal.ToString("N2", CultureInfo.InvariantCulture);

    public string InflowTotalDisplay =>
        "₹" + InflowTotal.ToString("N2", CultureInfo.InvariantCulture);

    public string OutflowTotalDisplay =>
        "₹" + OutflowTotal.ToString("N2", CultureInfo.InvariantCulture);

    public string NetMovementDisplay =>
        "₹" + NetMovement.ToString("N2", CultureInfo.InvariantCulture);

    public string RangeBalanceEffectDisplay =>
        "₹" + RangeBalanceEffect.ToString("N2", CultureInfo.InvariantCulture);

    public string DateRangeDisplay =>
        FromDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)
        + " IST to "
        + ToDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)
        + " IST";
}