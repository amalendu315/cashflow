namespace Cashflow.Web.ViewModels.CompanyMasters;

public class CompanyMasterIndexViewModel
{
    public IReadOnlyList<CompanyMasterListItemViewModel> Companies { get; set; }
        = new List<CompanyMasterListItemViewModel>();

    public int TotalCount => Companies.Count;

    public int ActiveCount => Companies.Count(company => company.IsActive);

    public int InactiveCount => Companies.Count(company => !company.IsActive);
}