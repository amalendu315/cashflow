using Cashflow.Web.ViewModels.PaymentRequests;
using System.Security.Claims;

namespace Cashflow.Web.Services;

public interface IPaymentRequestTrackingService
{
    Task<PaymentRequestIndexViewModel> GetIndexAsync(
        ClaimsPrincipal userPrincipal,
        CancellationToken cancellationToken = default);
}