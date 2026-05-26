using Cashflow.Web.Constants;
using Cashflow.Web.Data;
using Cashflow.Web.Models.Entities;
using Cashflow.Web.Models.Enums;
using Cashflow.Web.Utilities;
using Cashflow.Web.ViewModels.LedgerBalances;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace Cashflow.Web.Services;

public class LedgerBalanceService : ILedgerBalanceService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public LedgerBalanceService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
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

    public async Task<LedgerBalanceIndexViewModel> GetIndexAsync(
        int? ledgerMasterId,
        DateOnly? entryDate,
        CancellationToken cancellationToken = default)
    {
        LedgerBalanceIndexViewModel model = new()
        {
            LedgerMasterId = ledgerMasterId,
            EntryDate = entryDate ?? AppClock.TodayIst()
        };

        await PopulateLedgerOptionsAsync(model, cancellationToken);

        if (!ledgerMasterId.HasValue)
        {
            return model;
        }

        model.Balance = await GetDailyBalanceAsync(
            ledgerMasterId.Value,
            model.EntryDate,
            cancellationToken);

        model.Entries = await _dbContext.LedgerEntries
            .AsNoTracking()
            .Where(entry =>
                entry.LedgerMasterId == ledgerMasterId.Value &&
                entry.EntryDate == model.EntryDate)
            .OrderBy(entry => entry.EntryType)
            .ThenBy(entry => entry.CreatedAtUtc)
            .Select(entry => new LedgerEntryListItemViewModel
            {
                Id = entry.Id,
                EntryType = entry.EntryType,
                Amount = entry.Amount,
                Description = entry.Description,
                PaymentRequestId = entry.PaymentRequestId,
                CreatedByName = entry.CreatedByUser.FullName,
                CreatedByEmail = entry.CreatedByUser.Email ?? string.Empty,
                CreatedAtUtc = entry.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return model;
    }

    public async Task PopulateLedgerOptionsAsync(
        LedgerBalanceIndexViewModel model,
        CancellationToken cancellationToken = default)
    {
        List<SelectListItem> ledgerOptions = await _dbContext.LedgerMasters
            .AsNoTracking()
            .Where(ledger => ledger.IsActive)
            .OrderBy(ledger => ledger.LedgerName)
            .Select(ledger => new SelectListItem
            {
                Value = ledger.Id.ToString(CultureInfo.InvariantCulture),
                Text = ledger.LedgerName + " (" + ledger.LedgerCode + ")",
                Selected = model.LedgerMasterId.HasValue &&
                           ledger.Id == model.LedgerMasterId.Value
            })
            .ToListAsync(cancellationToken);

        model.LedgerOptions = ledgerOptions;
    }

    public async Task<LedgerEntryOperationResult> SaveOpeningBalanceAsync(
        OpeningBalanceFormViewModel model,
        ClaimsPrincipal adminPrincipal,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? adminUser = await GetValidAdminAsync(adminPrincipal);

        if (adminUser is null)
        {
            return LedgerEntryOperationResult.Failure("Admin user was not found.");
        }

        if (!model.LedgerMasterId.HasValue)
        {
            return LedgerEntryOperationResult.Failure("Ledger is required.");
        }

        if (!model.EntryDate.HasValue)
        {
            return LedgerEntryOperationResult.Failure("Business date is required.");
        }

        if (!model.Amount.HasValue || model.Amount.Value < 0)
        {
            return LedgerEntryOperationResult.Failure("Opening balance cannot be negative.");
        }

        bool activeLedgerExists = await ActiveLedgerExistsAsync(
            model.LedgerMasterId.Value,
            cancellationToken);

        if (!activeLedgerExists)
        {
            return LedgerEntryOperationResult.Failure("Please select a valid active ledger.");
        }

        LedgerEntry? existingOpening = await _dbContext.LedgerEntries
            .FirstOrDefaultAsync(
                entry =>
                    entry.LedgerMasterId == model.LedgerMasterId.Value &&
                    entry.EntryDate == model.EntryDate.Value &&
                    entry.EntryType == LedgerEntryType.OpeningBalance,
                cancellationToken);

        string? description = string.IsNullOrWhiteSpace(model.Description)
            ? "Opening balance."
            : model.Description.Trim();

        if (existingOpening is null)
        {
            LedgerEntry openingEntry = new()
            {
                LedgerMasterId = model.LedgerMasterId.Value,
                EntryDate = model.EntryDate.Value,
                EntryType = LedgerEntryType.OpeningBalance,
                Amount = model.Amount.Value,
                Description = description,
                CreatedByUserId = adminUser.Id,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.LedgerEntries.Add(openingEntry);
        }
        else
        {
            existingOpening.Amount = model.Amount.Value;
            existingOpening.Description = description;
            existingOpening.CreatedByUserId = adminUser.Id;
            existingOpening.CreatedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return LedgerEntryOperationResult.Success();
    }

    public async Task<LedgerEntryOperationResult> AddManualInflowAsync(
        ManualInflowFormViewModel model,
        ClaimsPrincipal adminPrincipal,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? adminUser = await GetValidAdminAsync(adminPrincipal);

        if (adminUser is null)
        {
            return LedgerEntryOperationResult.Failure("Admin user was not found.");
        }

        if (!model.LedgerMasterId.HasValue)
        {
            return LedgerEntryOperationResult.Failure("Ledger is required.");
        }

        if (!model.EntryDate.HasValue)
        {
            return LedgerEntryOperationResult.Failure("Business date is required.");
        }

        if (!model.Amount.HasValue || model.Amount.Value <= 0)
        {
            return LedgerEntryOperationResult.Failure("Inflow amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(model.Description))
        {
            return LedgerEntryOperationResult.Failure("Inflow description is required.");
        }

        bool activeLedgerExists = await ActiveLedgerExistsAsync(
            model.LedgerMasterId.Value,
            cancellationToken);

        if (!activeLedgerExists)
        {
            return LedgerEntryOperationResult.Failure("Please select a valid active ledger.");
        }

        LedgerEntry inflowEntry = new()
        {
            LedgerMasterId = model.LedgerMasterId.Value,
            EntryDate = model.EntryDate.Value,
            EntryType = LedgerEntryType.Inflow,
            Amount = model.Amount.Value,
            Description = model.Description.Trim(),
            CreatedByUserId = adminUser.Id,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.LedgerEntries.Add(inflowEntry);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return LedgerEntryOperationResult.Success();
    }

    public async Task<LedgerReportIndexViewModel> GetReportAsync(
    int? ledgerMasterId,
    DateOnly? fromDate,
    DateOnly? toDate,
    CancellationToken cancellationToken = default)
    {
        DateOnly resolvedFromDate = fromDate ?? AppClock.TodayIst();
        DateOnly resolvedToDate = toDate ?? AppClock.TodayIst();

        LedgerReportIndexViewModel model = new()
        {
            LedgerMasterId = ledgerMasterId,
            FromDate = resolvedFromDate,
            ToDate = resolvedToDate
        };

        List<SelectListItem> ledgerOptions = await _dbContext.LedgerMasters
            .AsNoTracking()
            .Where(ledger => ledger.IsActive)
            .OrderBy(ledger => ledger.LedgerName)
            .Select(ledger => new SelectListItem
            {
                Value = ledger.Id.ToString(CultureInfo.InvariantCulture),
                Text = ledger.LedgerName + " (" + ledger.LedgerCode + ")",
                Selected = ledgerMasterId.HasValue && ledger.Id == ledgerMasterId.Value
            })
            .ToListAsync(cancellationToken);

        model.LedgerOptions = ledgerOptions;

        if (!ledgerMasterId.HasValue || model.HasDateError)
        {
            return model;
        }

        var ledgerInfo = await _dbContext.LedgerMasters
            .AsNoTracking()
            .Where(ledger => ledger.Id == ledgerMasterId.Value)
            .Select(ledger => new
            {
                ledger.LedgerCode,
                ledger.LedgerName
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (ledgerInfo is null)
        {
            return model;
        }

        model.LedgerCode = ledgerInfo.LedgerCode;
        model.LedgerName = ledgerInfo.LedgerName;

        model.Entries = await _dbContext.LedgerEntries
            .AsNoTracking()
            .Where(entry =>
                entry.LedgerMasterId == ledgerMasterId.Value &&
                entry.EntryDate >= resolvedFromDate &&
                entry.EntryDate <= resolvedToDate)
            .OrderBy(entry => entry.EntryDate)
            .ThenBy(entry => entry.EntryType)
            .ThenBy(entry => entry.CreatedAtUtc)
            .Select(entry => new LedgerReportEntryViewModel
            {
                Id = entry.Id,
                EntryDate = entry.EntryDate,
                EntryType = entry.EntryType,
                Amount = entry.Amount,
                Description = entry.Description,
                PaymentRequestId = entry.PaymentRequestId,
                CreatedByName = entry.CreatedByUser.FullName,
                CreatedByEmail = entry.CreatedByUser.Email ?? string.Empty,
                CreatedAtUtc = entry.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return model;
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

    private async Task<ApplicationUser?> GetValidAdminAsync(ClaimsPrincipal adminPrincipal)
    {
        ApplicationUser? adminUser = await _userManager.GetUserAsync(adminPrincipal);

        if (adminUser is null || !adminUser.IsActive)
        {
            return null;
        }

        bool isAdmin = await _userManager.IsInRoleAsync(adminUser, AppRoles.Admin);

        return isAdmin ? adminUser : null;
    }

    private async Task<bool> ActiveLedgerExistsAsync(
        int ledgerMasterId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.LedgerMasters
            .AsNoTracking()
            .AnyAsync(
                ledger => ledger.Id == ledgerMasterId && ledger.IsActive,
                cancellationToken);
    }
}