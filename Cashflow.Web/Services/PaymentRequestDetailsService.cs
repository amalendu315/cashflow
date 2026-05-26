using Cashflow.Web.Constants;
using Cashflow.Web.Data;
using Cashflow.Web.Models.Entities;
using Cashflow.Web.ViewModels.PaymentRequests;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Cashflow.Web.Services;

public class PaymentRequestDetailsService : IPaymentRequestDetailsService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public PaymentRequestDetailsService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<PaymentRequestDetailsViewModel?> GetDetailsAsync(
        long id,
        ClaimsPrincipal userPrincipal,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(userPrincipal);

        if (currentUser is null || !currentUser.IsActive)
        {
            return null;
        }

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, AppRoles.Admin);
        bool isUser = await _userManager.IsInRoleAsync(currentUser, AppRoles.User);

        if (!isAdmin && !isUser)
        {
            return null;
        }

        if (isUser && !currentUser.CompanyId.HasValue)
        {
            return null;
        }

        var query = _dbContext.PaymentRequests
            .AsNoTracking()
            .Where(request => request.Id == id);

        if (isUser)
        {
            int companyId = currentUser.CompanyId!.Value;

            query = query.Where(request => request.CompanyId == companyId);
        }

        PaymentRequestDetailsViewModel? model = await query
            .Select(request => new PaymentRequestDetailsViewModel
            {
                Id = request.Id,
                CompanyCode = request.Company.CompanyCode,
                CompanyName = request.Company.CompanyName,
                VendorCode = request.VendorMaster.VendorCode,
                VendorName = request.VendorMaster.VendorName,
                RequestedAmount = request.RequestedAmount,
                ApprovedAmount = request.ApprovedAmount,
                Status = request.Status,
                Priority = request.Priority,
                ScheduledPaymentDate = request.ScheduledPaymentDate,
                RequestNotes = request.RequestNotes,
                ReviewNotes = request.ReviewNotes,
                RequestedByName = request.RequestedByUser.FullName,
                RequestedByEmail = request.RequestedByUser.Email ?? string.Empty,
                ReviewedByName = request.ReviewedByUser == null
                    ? null
                    : request.ReviewedByUser.FullName,
                ReviewedByEmail = request.ReviewedByUser == null
                    ? null
                    : request.ReviewedByUser.Email,
                RequestedAtUtc = request.RequestedAtUtc,
                ReviewedAtUtc = request.ReviewedAtUtc,
                ParentPaymentRequestId = request.ParentPaymentRequestId,
                IsAdminView = isAdmin
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (model is null)
        {
            return null;
        }

        if (model.ParentPaymentRequestId.HasValue)
        {
            model.ParentRequest = await GetRelatedRequestAsync(
                model.ParentPaymentRequestId.Value,
                currentUser,
                isAdmin,
                cancellationToken);
        }

        model.ChildRequests = await GetChildRequestsAsync(
            model.Id,
            currentUser,
            isAdmin,
            cancellationToken);

        model.LedgerOutflow = await _dbContext.LedgerEntries
            .AsNoTracking()
            .Where(entry => entry.PaymentRequestId == model.Id)
            .Select(entry => new PaymentRequestLedgerOutflowViewModel
            {
                LedgerEntryId = entry.Id,
                LedgerCode = entry.LedgerMaster.LedgerCode,
                LedgerName = entry.LedgerMaster.LedgerName,
                EntryDate = entry.EntryDate,
                Amount = entry.Amount,
                Description = entry.Description,
                CreatedByName = entry.CreatedByUser.FullName,
                CreatedByEmail = entry.CreatedByUser.Email ?? string.Empty,
                CreatedAtUtc = entry.CreatedAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        return model;
    }

    private async Task<RelatedPaymentRequestViewModel?> GetRelatedRequestAsync(
        long id,
        ApplicationUser currentUser,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.PaymentRequests
            .AsNoTracking()
            .Where(request => request.Id == id);

        if (!isAdmin)
        {
            query = query.Where(request => request.CompanyId == currentUser.CompanyId);
        }

        return await query
            .Select(request => new RelatedPaymentRequestViewModel
            {
                Id = request.Id,
                Status = request.Status,
                Priority = request.Priority,
                ScheduledPaymentDate = request.ScheduledPaymentDate,
                RequestedAmount = request.RequestedAmount,
                ApprovedAmount = request.ApprovedAmount,
                RequestedAtUtc = request.RequestedAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<RelatedPaymentRequestViewModel>> GetChildRequestsAsync(
        long parentId,
        ApplicationUser currentUser,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.PaymentRequests
            .AsNoTracking()
            .Where(request => request.ParentPaymentRequestId == parentId);

        if (!isAdmin)
        {
            query = query.Where(request => request.CompanyId == currentUser.CompanyId);
        }

        return await query
            .OrderBy(request => request.Id)
            .Select(request => new RelatedPaymentRequestViewModel
            {
                Id = request.Id,
                Status = request.Status,
                Priority = request.Priority,
                ScheduledPaymentDate = request.ScheduledPaymentDate,
                RequestedAmount = request.RequestedAmount,
                ApprovedAmount = request.ApprovedAmount,
                RequestedAtUtc = request.RequestedAtUtc
            })
            .ToListAsync(cancellationToken);
    }
}