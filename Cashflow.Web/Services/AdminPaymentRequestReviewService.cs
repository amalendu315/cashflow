using Cashflow.Web.Constants;
using Cashflow.Web.Data;
using Cashflow.Web.Models.Entities;
using Cashflow.Web.Models.Enums;
using Cashflow.Web.ViewModels.AdminPaymentRequests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace Cashflow.Web.Services;

public class AdminPaymentRequestReviewService : IAdminPaymentRequestReviewService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminPaymentRequestReviewService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<AdminPaymentRequestIndexViewModel> GetPendingIndexAsync(
        CancellationToken cancellationToken = default)
    {
        List<AdminPaymentRequestListItemViewModel> pendingRequests =
            await _dbContext.PaymentRequests
                .AsNoTracking()
                .Where(request => request.Status == PaymentRequestStatus.Pending)
                .OrderByDescending(request => request.Priority == PaymentPriority.High)
                .ThenBy(request => request.ScheduledPaymentDate)
                .ThenBy(request => request.RequestedAtUtc)
                .Select(request => new AdminPaymentRequestListItemViewModel
                {
                    Id = request.Id,
                    CompanyCode = request.Company.CompanyCode,
                    CompanyName = request.Company.CompanyName,
                    VendorCode = request.VendorMaster.VendorCode,
                    VendorName = request.VendorMaster.VendorName,
                    RequestedAmount = request.RequestedAmount,
                    Priority = request.Priority,
                    ScheduledPaymentDate = request.ScheduledPaymentDate,
                    RequestedByName = request.RequestedByUser.FullName,
                    RequestedByEmail = request.RequestedByUser.Email ?? string.Empty,
                    RequestedAtUtc = request.RequestedAtUtc,
                    RequestNotes = request.RequestNotes
                })
                .ToListAsync(cancellationToken);

        return new AdminPaymentRequestIndexViewModel
        {
            PendingRequests = pendingRequests
        };
    }

    public async Task<AdminPaymentRequestReviewViewModel?> GetForReviewAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        AdminPaymentRequestReviewViewModel? model =
            await _dbContext.PaymentRequests
                .AsNoTracking()
                .Where(request =>
                    request.Id == id &&
                    request.Status == PaymentRequestStatus.Pending)
                .Select(request => new AdminPaymentRequestReviewViewModel
                {
                    Id = request.Id,
                    CompanyCode = request.Company.CompanyCode,
                    CompanyName = request.Company.CompanyName,
                    VendorCode = request.VendorMaster.VendorCode,
                    VendorName = request.VendorMaster.VendorName,
                    RequestedAmount = request.RequestedAmount,
                    Priority = request.Priority,
                    ScheduledPaymentDate = request.ScheduledPaymentDate,
                    RequestNotes = request.RequestNotes,
                    RequestedByName = request.RequestedByUser.FullName,
                    RequestedByEmail = request.RequestedByUser.Email ?? string.Empty,
                    RequestedAtUtc = request.RequestedAtUtc,
                    SplitApprovedAmount = request.RequestedAmount,
                    SplitRemainderScheduledPaymentDate = request.ScheduledPaymentDate
                })
                .FirstOrDefaultAsync(cancellationToken);

        if (model is null)
        {
            return null;
        }

        await PopulateLedgerOptionsAsync(model, cancellationToken);

        return model;
    }

    public async Task PopulateLedgerOptionsAsync(
        AdminPaymentRequestReviewViewModel model,
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
                Selected = model.ApprovedLedgerMasterId.HasValue &&
                           ledger.Id == model.ApprovedLedgerMasterId.Value
            })
            .ToListAsync(cancellationToken);

        model.LedgerOptions = ledgerOptions;
    }

    public async Task<AdminPaymentRequestReviewResult> ApproveFullAsync(
        long id,
        AdminPaymentRequestReviewViewModel model,
        ClaimsPrincipal adminPrincipal,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? adminUser = await GetValidAdminAsync(adminPrincipal);

        if (adminUser is null)
        {
            return AdminPaymentRequestReviewResult.Failure("Admin user was not found.");
        }

        if (!model.ApprovedLedgerMasterId.HasValue)
        {
            return AdminPaymentRequestReviewResult.Failure("Ledger is required for approval.");
        }

        bool activeLedgerExists = await ActiveLedgerExistsAsync(
            model.ApprovedLedgerMasterId.Value,
            cancellationToken);

        if (!activeLedgerExists)
        {
            return AdminPaymentRequestReviewResult.Failure("Please select a valid active ledger.");
        }

        PaymentRequest? request = await _dbContext.PaymentRequests
            .FirstOrDefaultAsync(
                paymentRequest =>
                    paymentRequest.Id == id &&
                    paymentRequest.Status == PaymentRequestStatus.Pending,
                cancellationToken);

        if (request is null)
        {
            return AdminPaymentRequestReviewResult.Failure(
                "Pending payment request was not found.");
        }

        request.Status = PaymentRequestStatus.Approved;
        request.ApprovedLedgerMasterId = model.ApprovedLedgerMasterId.Value;
        request.ApprovedAmount = request.RequestedAmount;
        request.ReviewNotes = NormalizeNotes(model.ReviewNotes);
        request.ReviewedByUserId = adminUser.Id;
        request.ReviewedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return AdminPaymentRequestReviewResult.Success();
    }

    public async Task<AdminPaymentRequestReviewResult> RejectAsync(
        long id,
        AdminPaymentRequestReviewViewModel model,
        ClaimsPrincipal adminPrincipal,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? adminUser = await GetValidAdminAsync(adminPrincipal);

        if (adminUser is null)
        {
            return AdminPaymentRequestReviewResult.Failure("Admin user was not found.");
        }

        PaymentRequest? request = await _dbContext.PaymentRequests
            .FirstOrDefaultAsync(
                paymentRequest =>
                    paymentRequest.Id == id &&
                    paymentRequest.Status == PaymentRequestStatus.Pending,
                cancellationToken);

        if (request is null)
        {
            return AdminPaymentRequestReviewResult.Failure(
                "Pending payment request was not found.");
        }

        request.Status = PaymentRequestStatus.Rejected;
        request.ApprovedLedgerMasterId = null;
        request.ApprovedAmount = null;
        request.ReviewNotes = NormalizeNotes(model.ReviewNotes);
        request.ReviewedByUserId = adminUser.Id;
        request.ReviewedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return AdminPaymentRequestReviewResult.Success();
    }

    public async Task<AdminPaymentRequestReviewResult> SplitAsync(
        long id,
        AdminPaymentRequestReviewViewModel model,
        ClaimsPrincipal adminPrincipal,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? adminUser = await GetValidAdminAsync(adminPrincipal);

        if (adminUser is null)
        {
            return AdminPaymentRequestReviewResult.Failure("Admin user was not found.");
        }

        if (!model.ApprovedLedgerMasterId.HasValue)
        {
            return AdminPaymentRequestReviewResult.Failure("Ledger is required for split approval.");
        }

        if (!model.SplitApprovedAmount.HasValue)
        {
            return AdminPaymentRequestReviewResult.Failure("Approved amount is required for split.");
        }

        if (!model.SplitRemainderScheduledPaymentDate.HasValue)
        {
            return AdminPaymentRequestReviewResult.Failure(
                "Remainder scheduled payment date is required.");
        }

        bool activeLedgerExists = await ActiveLedgerExistsAsync(
            model.ApprovedLedgerMasterId.Value,
            cancellationToken);

        if (!activeLedgerExists)
        {
            return AdminPaymentRequestReviewResult.Failure("Please select a valid active ledger.");
        }

        PaymentRequest? request = await _dbContext.PaymentRequests
            .FirstOrDefaultAsync(
                paymentRequest =>
                    paymentRequest.Id == id &&
                    paymentRequest.Status == PaymentRequestStatus.Pending,
                cancellationToken);

        if (request is null)
        {
            return AdminPaymentRequestReviewResult.Failure(
                "Pending payment request was not found.");
        }

        decimal approvedAmount = model.SplitApprovedAmount.Value;

        if (approvedAmount <= 0)
        {
            return AdminPaymentRequestReviewResult.Failure(
                "Approved amount must be greater than zero.");
        }

        if (approvedAmount >= request.RequestedAmount)
        {
            return AdminPaymentRequestReviewResult.Failure(
                "For split payment, approved amount must be less than requested amount.");
        }

        decimal remainingAmount = request.RequestedAmount - approvedAmount;

        if (remainingAmount <= 0)
        {
            return AdminPaymentRequestReviewResult.Failure(
                "Split remainder must be greater than zero.");
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        request.Status = PaymentRequestStatus.Split;
        request.ApprovedLedgerMasterId = model.ApprovedLedgerMasterId.Value;
        request.ApprovedAmount = approvedAmount;
        request.ReviewNotes = NormalizeNotes(model.ReviewNotes);
        request.ReviewedByUserId = adminUser.Id;
        request.ReviewedAtUtc = DateTime.UtcNow;

        PaymentRequest childRequest = new()
        {
            CompanyId = request.CompanyId,
            VendorMasterId = request.VendorMasterId,
            RequestedAmount = remainingAmount,
            Status = PaymentRequestStatus.Pending,
            Priority = request.Priority,
            ScheduledPaymentDate = model.SplitRemainderScheduledPaymentDate.Value,
            RequestNotes = BuildSplitChildNotes(request.Id, request.RequestNotes),
            RequestedByUserId = request.RequestedByUserId,
            ParentPaymentRequestId = request.Id,
            RequestedAtUtc = DateTime.UtcNow
        };

        _dbContext.PaymentRequests.Add(childRequest);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return AdminPaymentRequestReviewResult.Success(childRequest.Id);
    }

    private async Task<ApplicationUser?> GetValidAdminAsync(ClaimsPrincipal adminPrincipal)
    {
        ApplicationUser? adminUser = await _userManager.GetUserAsync(adminPrincipal);

        if (adminUser is null)
        {
            return null;
        }

        if (!adminUser.IsActive)
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

    private static string? NormalizeNotes(string? notes)
    {
        return string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();
    }

    private static string BuildSplitChildNotes(long parentRequestId, string? originalNotes)
    {
        string prefix = $"Split balance from PR-{parentRequestId:D6}.";

        if (string.IsNullOrWhiteSpace(originalNotes))
        {
            return prefix;
        }

        string notes = prefix + " Original notes: " + originalNotes.Trim();

        return notes.Length <= 500
            ? notes
            : notes[..500];
    }
}