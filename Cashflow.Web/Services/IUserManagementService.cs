using Cashflow.Web.ViewModels.UserManagement;

namespace Cashflow.Web.Services;

public interface IUserManagementService
{
    Task<SubsidiaryUserIndexViewModel> GetIndexAsync(
        CancellationToken cancellationToken = default);

    Task PopulateCompanyOptionsAsync(
        SubsidiaryUserFormBaseViewModel model,
        CancellationToken cancellationToken = default);

    Task<UserManagementOperationResult> CreateAsync(
        SubsidiaryUserCreateViewModel model,
        CancellationToken cancellationToken = default);

    Task<SubsidiaryUserEditViewModel?> GetForEditAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<UserManagementOperationResult> UpdateAsync(
        string id,
        SubsidiaryUserEditViewModel model,
        CancellationToken cancellationToken = default);

    Task<UserManagementOperationResult> SetActiveStatusAsync(
        string id,
        bool isActive,
        CancellationToken cancellationToken = default);
}