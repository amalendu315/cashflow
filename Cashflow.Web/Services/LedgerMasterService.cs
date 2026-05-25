using Cashflow.Web.Data;
using Cashflow.Web.Models.Entities;
using Cashflow.Web.ViewModels.LedgerMasters;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Web.Services;

public class LedgerMasterService : ILedgerMasterService
{
    private readonly ApplicationDbContext _dbContext;

    public LedgerMasterService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LedgerMasterIndexViewModel> GetIndexAsync(
        CancellationToken cancellationToken = default)
    {
        List<LedgerMasterListItemViewModel> ledgers = await _dbContext.LedgerMasters
            .AsNoTracking()
            .OrderBy(ledger => ledger.LedgerName)
            .Select(ledger => new LedgerMasterListItemViewModel
            {
                Id = ledger.Id,
                LedgerCode = ledger.LedgerCode,
                LedgerName = ledger.LedgerName,
                Description = ledger.Description,
                IsActive = ledger.IsActive,
                CreatedAtUtc = ledger.CreatedAtUtc,
                ApprovedPaymentRequestCount = ledger.ApprovedPaymentRequests.Count
            })
            .ToListAsync(cancellationToken);

        return new LedgerMasterIndexViewModel
        {
            Ledgers = ledgers
        };
    }

    public async Task<LedgerMasterFormViewModel?> GetForEditAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.LedgerMasters
            .AsNoTracking()
            .Where(ledger => ledger.Id == id)
            .Select(ledger => new LedgerMasterFormViewModel
            {
                Id = ledger.Id,
                LedgerCode = ledger.LedgerCode,
                LedgerName = ledger.LedgerName,
                Description = ledger.Description,
                IsActive = ledger.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<LedgerMasterOperationResult> CreateAsync(
        LedgerMasterFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        string ledgerCode = model.LedgerCode.Trim();
        string ledgerName = model.LedgerName.Trim();
        string? description = string.IsNullOrWhiteSpace(model.Description)
            ? null
            : model.Description.Trim();

        bool duplicateCodeExists = await _dbContext.LedgerMasters
            .AnyAsync(
                ledger => ledger.LedgerCode == ledgerCode,
                cancellationToken);

        if (duplicateCodeExists)
        {
            return LedgerMasterOperationResult.Failure(
                "A ledger with this code already exists.");
        }

        LedgerMaster ledgerMaster = new()
        {
            LedgerCode = ledgerCode,
            LedgerName = ledgerName,
            Description = description,
            IsActive = model.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.LedgerMasters.Add(ledgerMaster);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return LedgerMasterOperationResult.Success();
    }

    public async Task<LedgerMasterOperationResult> UpdateAsync(
        int id,
        LedgerMasterFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        LedgerMaster? ledgerMaster = await _dbContext.LedgerMasters
            .FirstOrDefaultAsync(
                ledger => ledger.Id == id,
                cancellationToken);

        if (ledgerMaster is null)
        {
            return LedgerMasterOperationResult.Failure("Ledger was not found.");
        }

        string ledgerCode = model.LedgerCode.Trim();
        string ledgerName = model.LedgerName.Trim();
        string? description = string.IsNullOrWhiteSpace(model.Description)
            ? null
            : model.Description.Trim();

        bool duplicateCodeExists = await _dbContext.LedgerMasters
            .AnyAsync(
                ledger => ledger.Id != id && ledger.LedgerCode == ledgerCode,
                cancellationToken);

        if (duplicateCodeExists)
        {
            return LedgerMasterOperationResult.Failure(
                "Another ledger with this code already exists.");
        }

        ledgerMaster.LedgerCode = ledgerCode;
        ledgerMaster.LedgerName = ledgerName;
        ledgerMaster.Description = description;
        ledgerMaster.IsActive = model.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return LedgerMasterOperationResult.Success();
    }

    public async Task<LedgerMasterOperationResult> SetActiveStatusAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        LedgerMaster? ledgerMaster = await _dbContext.LedgerMasters
            .FirstOrDefaultAsync(
                ledger => ledger.Id == id,
                cancellationToken);

        if (ledgerMaster is null)
        {
            return LedgerMasterOperationResult.Failure("Ledger was not found.");
        }

        ledgerMaster.IsActive = isActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return LedgerMasterOperationResult.Success();
    }
}