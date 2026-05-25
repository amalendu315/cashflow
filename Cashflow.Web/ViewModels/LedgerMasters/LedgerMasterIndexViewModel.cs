namespace Cashflow.Web.ViewModels.LedgerMasters;

public class LedgerMasterIndexViewModel
{
    public IReadOnlyList<LedgerMasterListItemViewModel> Ledgers { get; set; }
        = new List<LedgerMasterListItemViewModel>();

    public int TotalCount => Ledgers.Count;

    public int ActiveCount => Ledgers.Count(ledger => ledger.IsActive);

    public int InactiveCount => Ledgers.Count(ledger => !ledger.IsActive);
}