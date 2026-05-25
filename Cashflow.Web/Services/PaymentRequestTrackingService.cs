using Cashflow.Web.Constants;
using Cashflow.Web.Data;
using Cashflow.Web.Models.Entities;
using Cashflow.Web.ViewModels.PaymentRequests;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Cashflow.Web.Services;

public class PaymentRequestTrackingService : IPaymentRequestTrackingService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public PaymentRequestTrackingService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<PaymentRequestIndexViewModel> GetIndexAsync(
        ClaimsPrincipal userPrincipal,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(userPrincipal);

        if (currentUser is null)
        {
            return new PaymentRequestIndexViewModel
            {
                ErrorMessage = "Signed-in user was not found."
            };
        }

        bool isSubsidiaryUser = await _userManager.IsInRoleAsync(currentUser, AppRoles.User);

        if (!isSubsidiaryUser)
        {
            return new PaymentRequestIndexViewModel
            {
                ErrorMessage = "Only subsidiary users can track payment requests from this page."
            };
        }

        if (!currentUser.IsActive)
        {
            return new PaymentRequestIndexViewModel
            {
                ErrorMessage = "Your user account is inactive."
            };
        }

        if (!currentUser.CompanyId.HasValue)
        {
            return new PaymentRequestIndexViewModel
            {
                ErrorMessage = "Your user account is not assigned to a company."
            };
        }

        var company = await _dbContext.CompanyMasters
            .AsNoTracking()
            .Where(company => company.Id == currentUser.CompanyId.Value)
            .Select(company => new
            {
                company.Id,
                company.CompanyCode,
                company.CompanyName,
                company.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (company is null)
        {
            return new PaymentRequestIndexViewModel
            {
                ErrorMessage = "Your assigned company was not found."
            };
        }

        if (!company.IsActive)
        {
            return new PaymentRequestIndexViewModel
            {
                CompanyCode = company.CompanyCode,
                CompanyName = company.CompanyName,
                ErrorMessage = "Your assigned company is inactive."
            };
        }

        List<PaymentRequestListItemViewModel> requests = await _dbContext.PaymentRequests
            .AsNoTracking()
            .Where(request => request.CompanyId == currentUser.CompanyId.Value)
            .OrderByDescending(request => request.RequestedAtUtc)
            .ThenByDescending(request => request.Id)
            .Select(request => new PaymentRequestListItemViewModel
            {
                Id = request.Id,
                VendorCode = request.VendorMaster.VendorCode,
                VendorName = request.VendorMaster.VendorName,
                RequestedAmount = request.RequestedAmount,
                ApprovedAmount = request.ApprovedAmount,
                Status = request.Status,
                Priority = request.Priority,
                RequestNotes = request.RequestNotes,
                ReviewNotes = request.ReviewNotes,
                RequestedByName = request.RequestedByUser.FullName,
                RequestedByEmail = request.RequestedByUser.Email ?? string.Empty,
                RequestedAtUtc = request.RequestedAtUtc,
                ReviewedAtUtc = request.ReviewedAtUtc,
                ScheduledPaymentDate = request.ScheduledPaymentDate,
                ParentPaymentRequestId = request.ParentPaymentRequestId
            })
            .ToListAsync(cancellationToken);

        return new PaymentRequestIndexViewModel
        {
            CompanyCode = company.CompanyCode,
            CompanyName = company.CompanyName,
            Requests = requests
        };
    }
}