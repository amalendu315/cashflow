using Cashflow.Web.Models.Entities;
using Cashflow.Web.ViewModels.LedgerBalances;
using System.Security.Claims;

namespace Cashflow.Web.Services;

public interface ILedgerBalanceService
{
    Task<LedgerDailyBalanceViewModel?> GetDailyBalanceAsync(
        int ledgerMasterId,
        DateOnly entryDate,
        CancellationToken cancellationToken = default);

    Task<LedgerBalanceIndexViewModel> GetIndexAsync(
        int? ledgerMasterId,
        DateOnly? entryDate,
        CancellationToken cancellationToken = default);

    Task PopulateLedgerOptionsAsync(
        LedgerBalanceIndexViewModel model,
        CancellationToken cancellationToken = default);

    Task<LedgerEntryOperationResult> SaveOpeningBalanceAsync(
        OpeningBalanceFormViewModel model,
        ClaimsPrincipal adminPrincipal,
        CancellationToken cancellationToken = default);

    Task<LedgerEntryOperationResult> AddManualInflowAsync(
        ManualInflowFormViewModel model,
        ClaimsPrincipal adminPrincipal,
        CancellationToken cancellationToken = default);

    Task<LedgerEntryOperationResult> QueueApprovedPaymentOutflowAsync(
        PaymentRequest paymentRequest,
        string adminUserId,
        CancellationToken cancellationToken = default);
}