using Cashflow.Web.ViewModels.PaymentRequests;
using System.Security.Claims;

namespace Cashflow.Web.Services;

public interface IPaymentRequestSubmissionService
{
    Task PopulateCreateFormOptionsAsync(
        PaymentRequestCreateViewModel model,
        CancellationToken cancellationToken = default);

    Task<PaymentRequestOperationResult> CreateAsync(
        PaymentRequestCreateViewModel model,
        ClaimsPrincipal userPrincipal,
        CancellationToken cancellationToken = default);
}