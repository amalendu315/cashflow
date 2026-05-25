using Cashflow.Web.ViewModels.VendorMasters;

namespace Cashflow.Web.Services;

public interface IVendorMasterService
{
    Task<VendorMasterIndexViewModel> GetIndexAsync(CancellationToken cancellationToken = default);

    Task<VendorMasterFormViewModel?> GetForEditAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<VendorMasterOperationResult> CreateAsync(
        VendorMasterFormViewModel model,
        CancellationToken cancellationToken = default);

    Task<VendorMasterOperationResult> UpdateAsync(
        int id,
        VendorMasterFormViewModel model,
        CancellationToken cancellationToken = default);

    Task<VendorMasterOperationResult> SetActiveStatusAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default);
}