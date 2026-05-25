namespace Cashflow.Web.ViewModels.VendorMasters;

public class VendorMasterIndexViewModel
{
    public IReadOnlyList<VendorMasterListItemViewModel> Vendors { get; set; }
        = new List<VendorMasterListItemViewModel>();

    public int TotalCount => Vendors.Count;

    public int ActiveCount => Vendors.Count(vendor => vendor.IsActive);

    public int InactiveCount => Vendors.Count(vendor => !vendor.IsActive);
}