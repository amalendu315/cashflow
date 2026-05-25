using Cashflow.Web.ViewModels.AdminPaymentRequests;
using System.Security.Claims;

namespace Cashflow.Web.Services;

public interface IAdminPaymentRequestReviewService
{
    Task<AdminPaymentRequestIndexViewModel> GetPendingIndexAsync(
        CancellationToken cancellationToken = default);

    Task<AdminPaymentRequestReviewViewModel?> GetForReviewAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task PopulateLedgerOptionsAsync(
        AdminPaymentRequestReviewViewModel model,
        CancellationToken cancellationToken = default);

    Task<AdminPaymentRequestReviewResult> ApproveFullAsync(
        long id,
        AdminPaymentRequestReviewViewModel model,
        ClaimsPrincipal adminPrincipal,
        CancellationToken cancellationToken = default);

    Task<AdminPaymentRequestReviewResult> RejectAsync(
        long id,
        AdminPaymentRequestReviewViewModel model,
        ClaimsPrincipal adminPrincipal,
        CancellationToken cancellationToken = default);

    Task<AdminPaymentRequestReviewResult> SplitAsync(
        long id,
        AdminPaymentRequestReviewViewModel model,
        ClaimsPrincipal adminPrincipal,
        CancellationToken cancellationToken = default);
}