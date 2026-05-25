using Cashflow.Web.Data;
using Cashflow.Web.Models.Entities;
using Cashflow.Web.ViewModels.CompanyMasters;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Web.Services;

public class CompanyMasterService : ICompanyMasterService
{
    private readonly ApplicationDbContext _dbContext;

    public CompanyMasterService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CompanyMasterIndexViewModel> GetIndexAsync(
        CancellationToken cancellationToken = default)
    {
        List<CompanyMasterListItemViewModel> companies = await _dbContext.CompanyMasters
            .AsNoTracking()
            .OrderBy(company => company.CompanyName)
            .Select(company => new CompanyMasterListItemViewModel
            {
                Id = company.Id,
                CompanyCode = company.CompanyCode,
                CompanyName = company.CompanyName,
                IsActive = company.IsActive,
                CreatedAtUtc = company.CreatedAtUtc,
                UserCount = company.Users.Count,
                PaymentRequestCount = company.PaymentRequests.Count
            })
            .ToListAsync(cancellationToken);

        return new CompanyMasterIndexViewModel
        {
            Companies = companies
        };
    }

    public async Task<CompanyMasterFormViewModel?> GetForEditAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CompanyMasters
            .AsNoTracking()
            .Where(company => company.Id == id)
            .Select(company => new CompanyMasterFormViewModel
            {
                Id = company.Id,
                CompanyCode = company.CompanyCode,
                CompanyName = company.CompanyName,
                IsActive = company.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CompanyMasterOperationResult> CreateAsync(
        CompanyMasterFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        string companyCode = model.CompanyCode.Trim();
        string companyName = model.CompanyName.Trim();

        bool duplicateCodeExists = await _dbContext.CompanyMasters
            .AnyAsync(
                company => company.CompanyCode == companyCode,
                cancellationToken);

        if (duplicateCodeExists)
        {
            return CompanyMasterOperationResult.Failure(
                "A company with this code already exists.");
        }

        CompanyMaster companyMaster = new()
        {
            CompanyCode = companyCode,
            CompanyName = companyName,
            IsActive = model.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.CompanyMasters.Add(companyMaster);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CompanyMasterOperationResult.Success();
    }

    public async Task<CompanyMasterOperationResult> UpdateAsync(
        int id,
        CompanyMasterFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        CompanyMaster? companyMaster = await _dbContext.CompanyMasters
            .FirstOrDefaultAsync(
                company => company.Id == id,
                cancellationToken);

        if (companyMaster is null)
        {
            return CompanyMasterOperationResult.Failure("Company was not found.");
        }

        string companyCode = model.CompanyCode.Trim();
        string companyName = model.CompanyName.Trim();

        bool duplicateCodeExists = await _dbContext.CompanyMasters
            .AnyAsync(
                company => company.Id != id && company.CompanyCode == companyCode,
                cancellationToken);

        if (duplicateCodeExists)
        {
            return CompanyMasterOperationResult.Failure(
                "Another company with this code already exists.");
        }

        companyMaster.CompanyCode = companyCode;
        companyMaster.CompanyName = companyName;
        companyMaster.IsActive = model.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CompanyMasterOperationResult.Success();
    }

    public async Task<CompanyMasterOperationResult> SetActiveStatusAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        CompanyMaster? companyMaster = await _dbContext.CompanyMasters
            .FirstOrDefaultAsync(
                company => company.Id == id,
                cancellationToken);

        if (companyMaster is null)
        {
            return CompanyMasterOperationResult.Failure("Company was not found.");
        }

        companyMaster.IsActive = isActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CompanyMasterOperationResult.Success();
    }
}