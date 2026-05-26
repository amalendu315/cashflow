using Cashflow.Web.ViewModels.PaymentRequests;
using System.Security.Claims;

namespace Cashflow.Web.Services;

public interface IPaymentRequestDetailsService
{
    Task<PaymentRequestDetailsViewModel?> GetDetailsAsync(
        long id,
        ClaimsPrincipal userPrincipal,
        CancellationToken cancellationToken = default);
}