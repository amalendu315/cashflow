using Cashflow.Web.Models.Entities;
using Cashflow.Web.ViewModels.LedgerBalances;

namespace Cashflow.Web.Services;

public interface ILedgerBalanceService
{
    Task<LedgerDailyBalanceViewModel?> GetDailyBalanceAsync(
        int ledgerMasterId,
        DateOnly entryDate,
        CancellationToken cancellationToken = default);

    Task<LedgerEntryOperationResult> QueueApprovedPaymentOutflowAsync(
        PaymentRequest paymentRequest,
        string adminUserId,
        CancellationToken cancellationToken = default);
}