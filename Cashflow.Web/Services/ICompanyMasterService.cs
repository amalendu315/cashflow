using Cashflow.Web.ViewModels.CompanyMasters;

namespace Cashflow.Web.Services;

public interface ICompanyMasterService
{
    Task<CompanyMasterIndexViewModel> GetIndexAsync(CancellationToken cancellationToken = default);

    Task<CompanyMasterFormViewModel?> GetForEditAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<CompanyMasterOperationResult> CreateAsync(
        CompanyMasterFormViewModel model,
        CancellationToken cancellationToken = default);

    Task<CompanyMasterOperationResult> UpdateAsync(
        int id,
        CompanyMasterFormViewModel model,
        CancellationToken cancellationToken = default);

    Task<CompanyMasterOperationResult> SetActiveStatusAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default);
}