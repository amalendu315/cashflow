using Cashflow.Web.Data;
using Cashflow.Web.Models.Entities;
using Cashflow.Web.Models.Enums;
using Cashflow.Web.ViewModels.LedgerBalances;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Web.Services;

public class LedgerBalanceService : ILedgerBalanceService
{
    private readonly ApplicationDbContext _dbContext;

    public LedgerBalanceService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LedgerDailyBalanceViewModel?> GetDailyBalanceAsync(
        int ledgerMasterId,
        DateOnly entryDate,
        CancellationToken cancellationToken = default)
    {
        var ledger = await _dbContext.LedgerMasters
            .AsNoTracking()
            .Where(ledger => ledger.Id == ledgerMasterId)
            .Select(ledger => new
            {
                ledger.Id,
                ledger.LedgerCode,
                ledger.LedgerName
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (ledger is null)
        {
            return null;
        }

        List<LedgerEntry> entries = await _dbContext.LedgerEntries
            .AsNoTracking()
            .Where(entry =>
                entry.LedgerMasterId == ledgerMasterId &&
                entry.EntryDate == entryDate)
            .ToListAsync(cancellationToken);

        decimal openingBalance = entries
            .Where(entry => entry.EntryType == LedgerEntryType.OpeningBalance)
            .Sum(entry => entry.Amount);

        decimal totalInflow = entries
            .Where(entry => entry.EntryType == LedgerEntryType.Inflow)
            .Sum(entry => entry.Amount);

        decimal totalOutflow = entries
            .Where(entry => entry.EntryType == LedgerEntryType.Outflow)
            .Sum(entry => entry.Amount);

        return new LedgerDailyBalanceViewModel
        {
            LedgerMasterId = ledger.Id,
            LedgerCode = ledger.LedgerCode,
            LedgerName = ledger.LedgerName,
            EntryDate = entryDate,
            HasOpeningBalance = entries.Any(entry => entry.EntryType == LedgerEntryType.OpeningBalance),
            OpeningBalance = openingBalance,
            TotalInflow = totalInflow,
            TotalOutflow = totalOutflow
        };
    }

    public async Task<LedgerEntryOperationResult> QueueApprovedPaymentOutflowAsync(
        PaymentRequest paymentRequest,
        string adminUserId,
        CancellationToken cancellationToken = default)
    {
        if (paymentRequest.ApprovedLedgerMasterId is null)
        {
            return LedgerEntryOperationResult.Failure(
                "Approved ledger is required before creating an outflow.");
        }

        if (paymentRequest.ApprovedAmount is null || paymentRequest.ApprovedAmount.Value <= 0)
        {
            return LedgerEntryOperationResult.Failure(
                "Approved amount must be greater than zero before creating an outflow.");
        }

        bool activeLedgerExists = await _dbContext.LedgerMasters
            .AsNoTracking()
            .AnyAsync(
                ledger =>
                    ledger.Id == paymentRequest.ApprovedLedgerMasterId.Value &&
                    ledger.IsActive,
                cancellationToken);

        if (!activeLedgerExists)
        {
            return LedgerEntryOperationResult.Failure(
                "Approved ledger is inactive or unavailable.");
        }

        bool outflowAlreadyExists = await _dbContext.LedgerEntries
            .AsNoTracking()
            .AnyAsync(
                entry => entry.PaymentRequestId == paymentRequest.Id,
                cancellationToken);

        if (outflowAlreadyExists)
        {
            return LedgerEntryOperationResult.Failure(
                "An outflow has already been recorded for this payment request.");
        }

        LedgerEntry outflowEntry = new()
        {
            LedgerMasterId = paymentRequest.ApprovedLedgerMasterId.Value,
            EntryDate = paymentRequest.ScheduledPaymentDate,
            EntryType = LedgerEntryType.Outflow,
            Amount = paymentRequest.ApprovedAmount.Value,
            Description = $"Payment request PR-{paymentRequest.Id:D6} approved outflow.",
            PaymentRequestId = paymentRequest.Id,
            CreatedByUserId = adminUserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.LedgerEntries.Add(outflowEntry);

        return LedgerEntryOperationResult.Success();
    }
}