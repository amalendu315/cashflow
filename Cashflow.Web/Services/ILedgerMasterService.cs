using Cashflow.Web.ViewModels.LedgerMasters;

namespace Cashflow.Web.Services;

public interface ILedgerMasterService
{
    Task<LedgerMasterIndexViewModel> GetIndexAsync(CancellationToken cancellationToken = default);

    Task<LedgerMasterFormViewModel?> GetForEditAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<LedgerMasterOperationResult> CreateAsync(
        LedgerMasterFormViewModel model,
        CancellationToken cancellationToken = default);

    Task<LedgerMasterOperationResult> UpdateAsync(
        int id,
        LedgerMasterFormViewModel model,
        CancellationToken cancellationToken = default);

    Task<LedgerMasterOperationResult> SetActiveStatusAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default);
}