using Cashflow.Web.Data;
using Cashflow.Web.Models.Entities;
using Cashflow.Web.ViewModels.VendorMasters;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Web.Services;

public class VendorMasterService : IVendorMasterService
{
    private readonly ApplicationDbContext _dbContext;

    public VendorMasterService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VendorMasterIndexViewModel> GetIndexAsync(
        CancellationToken cancellationToken = default)
    {
        List<VendorMasterListItemViewModel> vendors = await _dbContext.VendorMasters
            .AsNoTracking()
            .OrderBy(vendor => vendor.VendorName)
            .Select(vendor => new VendorMasterListItemViewModel
            {
                Id = vendor.Id,
                VendorCode = vendor.VendorCode,
                VendorName = vendor.VendorName,
                GstNumber = vendor.GstNumber,
                IsActive = vendor.IsActive,
                CreatedAtUtc = vendor.CreatedAtUtc,
                PaymentRequestCount = vendor.PaymentRequests.Count
            })
            .ToListAsync(cancellationToken);

        return new VendorMasterIndexViewModel
        {
            Vendors = vendors
        };
    }

    public async Task<VendorMasterFormViewModel?> GetForEditAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.VendorMasters
            .AsNoTracking()
            .Where(vendor => vendor.Id == id)
            .Select(vendor => new VendorMasterFormViewModel
            {
                Id = vendor.Id,
                VendorCode = vendor.VendorCode,
                VendorName = vendor.VendorName,
                GstNumber = vendor.GstNumber,
                IsActive = vendor.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<VendorMasterOperationResult> CreateAsync(
        VendorMasterFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        string vendorCode = model.VendorCode.Trim();
        string vendorName = model.VendorName.Trim();
        string? gstNumber = string.IsNullOrWhiteSpace(model.GstNumber)
            ? null
            : model.GstNumber.Trim();

        bool duplicateCodeExists = await _dbContext.VendorMasters
            .AnyAsync(
                vendor => vendor.VendorCode == vendorCode,
                cancellationToken);

        if (duplicateCodeExists)
        {
            return VendorMasterOperationResult.Failure(
                "A vendor with this code already exists.");
        }

        VendorMaster vendorMaster = new()
        {
            VendorCode = vendorCode,
            VendorName = vendorName,
            GstNumber = gstNumber,
            IsActive = model.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.VendorMasters.Add(vendorMaster);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return VendorMasterOperationResult.Success();
    }

    public async Task<VendorMasterOperationResult> UpdateAsync(
        int id,
        VendorMasterFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        VendorMaster? vendorMaster = await _dbContext.VendorMasters
            .FirstOrDefaultAsync(
                vendor => vendor.Id == id,
                cancellationToken);

        if (vendorMaster is null)
        {
            return VendorMasterOperationResult.Failure("Vendor was not found.");
        }

        string vendorCode = model.VendorCode.Trim();
        string vendorName = model.VendorName.Trim();
        string? gstNumber = string.IsNullOrWhiteSpace(model.GstNumber)
            ? null
            : model.GstNumber.Trim();

        bool duplicateCodeExists = await _dbContext.VendorMasters
            .AnyAsync(
                vendor => vendor.Id != id && vendor.VendorCode == vendorCode,
                cancellationToken);

        if (duplicateCodeExists)
        {
            return VendorMasterOperationResult.Failure(
                "Another vendor with this code already exists.");
        }

        vendorMaster.VendorCode = vendorCode;
        vendorMaster.VendorName = vendorName;
        vendorMaster.GstNumber = gstNumber;
        vendorMaster.IsActive = model.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return VendorMasterOperationResult.Success();
    }

    public async Task<VendorMasterOperationResult> SetActiveStatusAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        VendorMaster? vendorMaster = await _dbContext.VendorMasters
            .FirstOrDefaultAsync(
                vendor => vendor.Id == id,
                cancellationToken);

        if (vendorMaster is null)
        {
            return VendorMasterOperationResult.Failure("Vendor was not found.");
        }

        vendorMaster.IsActive = isActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return VendorMasterOperationResult.Success();
    }
}