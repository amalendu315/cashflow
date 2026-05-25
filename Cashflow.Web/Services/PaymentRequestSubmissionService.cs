using Cashflow.Web.Constants;
using Cashflow.Web.Data;
using Cashflow.Web.Models.Entities;
using Cashflow.Web.Models.Enums;
using Cashflow.Web.ViewModels.PaymentRequests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace Cashflow.Web.Services;

public class PaymentRequestSubmissionService : IPaymentRequestSubmissionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public PaymentRequestSubmissionService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task PopulateCreateFormOptionsAsync(
        PaymentRequestCreateViewModel model,
        CancellationToken cancellationToken = default)
    {
        await PopulateVendorOptionsAsync(model, cancellationToken);
        PopulatePriorityOptions(model);
    }

    private async Task PopulateVendorOptionsAsync(
        PaymentRequestCreateViewModel model,
        CancellationToken cancellationToken = default)
    {
        List<SelectListItem> vendorOptions = await _dbContext.VendorMasters
            .AsNoTracking()
            .Where(vendor => vendor.IsActive)
            .OrderBy(vendor => vendor.VendorName)
            .Select(vendor => new SelectListItem
            {
                Value = vendor.Id.ToString(CultureInfo.InvariantCulture),
                Text = vendor.VendorName + " (" + vendor.VendorCode + ")",
                Selected = model.VendorMasterId.HasValue
                    && vendor.Id == model.VendorMasterId.Value
            })
            .ToListAsync(cancellationToken);

        model.VendorOptions = vendorOptions;
    }

    private static void PopulatePriorityOptions(PaymentRequestCreateViewModel model)
    {
        PaymentPriority selectedPriority = model.Priority ?? PaymentPriority.Normal;

        model.PriorityOptions = new List<SelectListItem>
    {
        new()
        {
            Value = PaymentPriority.Normal.ToString(),
            Text = "Normal",
            Selected = selectedPriority == PaymentPriority.Normal
        },
        new()
        {
            Value = PaymentPriority.Medium.ToString(),
            Text = "Medium",
            Selected = selectedPriority == PaymentPriority.Medium
        },
        new()
        {
            Value = PaymentPriority.High.ToString(),
            Text = "High",
            Selected = selectedPriority == PaymentPriority.High
        }
    };
    }

    public async Task<PaymentRequestOperationResult> CreateAsync(
        PaymentRequestCreateViewModel model,
        ClaimsPrincipal userPrincipal,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(userPrincipal);

        if (currentUser is null)
        {
            return PaymentRequestOperationResult.Failure("Signed-in user was not found.");
        }

        bool isSubsidiaryUser = await _userManager.IsInRoleAsync(currentUser, AppRoles.User);

        if (!isSubsidiaryUser)
        {
            return PaymentRequestOperationResult.Failure(
                "Only subsidiary users can submit payment requests.");
        }

        if (!currentUser.IsActive)
        {
            return PaymentRequestOperationResult.Failure("Your user account is inactive.");
        }

        if (!currentUser.CompanyId.HasValue)
        {
            return PaymentRequestOperationResult.Failure(
                "Your user account is not assigned to a company.");
        }

        if (!model.VendorMasterId.HasValue)
        {
            return PaymentRequestOperationResult.Failure("Vendor is required.");
        }

        if (!model.RequestedAmount.HasValue || model.RequestedAmount.Value <= 0)
        {
            return PaymentRequestOperationResult.Failure(
                "Requested amount must be greater than zero.");
        }

        if (!model.Priority.HasValue || !Enum.IsDefined(typeof(PaymentPriority), model.Priority.Value))
        {
            return PaymentRequestOperationResult.Failure(
                "Please select a valid payment priority.");
        }

        bool companyExists = await _dbContext.CompanyMasters
            .AsNoTracking()
            .AnyAsync(
                company => company.Id == currentUser.CompanyId.Value && company.IsActive,
                cancellationToken);

        if (!companyExists)
        {
            return PaymentRequestOperationResult.Failure(
                "Your assigned company is inactive or unavailable.");
        }

        bool activeVendorExists = await _dbContext.VendorMasters
            .AsNoTracking()
            .AnyAsync(
                vendor => vendor.Id == model.VendorMasterId.Value && vendor.IsActive,
                cancellationToken);

        if (!activeVendorExists)
        {
            return PaymentRequestOperationResult.Failure(
                "Please select a valid active vendor.");
        }

        string? requestNotes = string.IsNullOrWhiteSpace(model.RequestNotes)
            ? null
            : model.RequestNotes.Trim();

        PaymentRequest paymentRequest = new()
        {
            CompanyId = currentUser.CompanyId.Value,
            VendorMasterId = model.VendorMasterId.Value,
            RequestedAmount = model.RequestedAmount.Value,
            Status = PaymentRequestStatus.Pending,
            Priority = model.Priority.Value,
            RequestNotes = requestNotes,
            RequestedByUserId = currentUser.Id,
            RequestedAtUtc = DateTime.UtcNow
        };

        _dbContext.PaymentRequests.Add(paymentRequest);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return PaymentRequestOperationResult.Success(paymentRequest.Id);
    }
}