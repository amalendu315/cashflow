using System.Globalization;

namespace Cashflow.Web.ViewModels.LedgerBalances;

public class LedgerDailyBalanceViewModel
{
    public int LedgerMasterId { get; set; }

    public string LedgerCode { get; set; } = string.Empty;

    public string LedgerName { get; set; } = string.Empty;

    public DateOnly EntryDate { get; set; }

    public bool HasOpeningBalance { get; set; }

    public decimal OpeningBalance { get; set; }

    public decimal TotalInflow { get; set; }

    public decimal TotalOutflow { get; set; }

    public decimal ClosingBalance => OpeningBalance + TotalInflow - TotalOutflow;

    public string EntryDateDisplay =>
        EntryDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + " IST";

    public string OpeningBalanceDisplay =>
        "₹" + OpeningBalance.ToString("N2", CultureInfo.InvariantCulture);

    public string TotalInflowDisplay =>
        "₹" + TotalInflow.ToString("N2", CultureInfo.InvariantCulture);

    public string TotalOutflowDisplay =>
        "₹" + TotalOutflow.ToString("N2", CultureInfo.InvariantCulture);

    public string ClosingBalanceDisplay =>
        "₹" + ClosingBalance.ToString("N2", CultureInfo.InvariantCulture);
}