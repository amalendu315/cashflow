using Cashflow.Web.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Cashflow.Web.ViewModels.LedgerBalances;

public class LedgerBalanceIndexViewModel
{
    [Required]
    [Display(Name = "Ledger")]
    public int? LedgerMasterId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Business Date")]
    public DateOnly EntryDate { get; set; } = AppClock.TodayIst();

    public IReadOnlyList<SelectListItem> LedgerOptions { get; set; }
        = new List<SelectListItem>();

    public LedgerDailyBalanceViewModel? Balance { get; set; }

    public IReadOnlyList<LedgerEntryListItemViewModel> Entries { get; set; }
        = new List<LedgerEntryListItemViewModel>();

    public bool HasLedgerSelected => LedgerMasterId.HasValue;

    public bool HasBalance => Balance is not null;

    public bool HasEntries => Entries.Any();

    public string EntryDateDisplay =>
        EntryDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + " IST";
}