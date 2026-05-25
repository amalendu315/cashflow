using Cashflow.Web.Constants;
using Cashflow.Web.Data;
using Cashflow.Web.Models.Entities;
using Cashflow.Web.ViewModels.UserManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Cashflow.Web.Services;

public class UserManagementService : IUserManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserManagementService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<SubsidiaryUserIndexViewModel> GetIndexAsync(
        CancellationToken cancellationToken = default)
    {
        IList<ApplicationUser> usersInRole =
            await _userManager.GetUsersInRoleAsync(AppRoles.User);

        string[] userIds = usersInRole
            .Select(user => user.Id)
            .ToArray();

        List<SubsidiaryUserListItemViewModel> users = await _dbContext.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .OrderBy(user => user.FullName)
            .Select(user => new SubsidiaryUserListItemViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                CompanyName = user.Company == null ? "Unassigned" : user.Company.CompanyName,
                CompanyCode = user.Company == null ? string.Empty : user.Company.CompanyCode,
                IsActive = user.IsActive,
                CreatedAtUtc = user.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new SubsidiaryUserIndexViewModel
        {
            Users = users
        };
    }

    public async Task PopulateCompanyOptionsAsync(
        SubsidiaryUserFormBaseViewModel model,
        CancellationToken cancellationToken = default)
    {
        List<SelectListItem> companyOptions = await _dbContext.CompanyMasters
            .AsNoTracking()
            .Where(company => company.IsActive)
            .OrderBy(company => company.CompanyName)
            .Select(company => new SelectListItem
            {
                Value = company.Id.ToString(CultureInfo.InvariantCulture),
                Text = company.CompanyName + " (" + company.CompanyCode + ")",
                Selected = model.CompanyId.HasValue && company.Id == model.CompanyId.Value
            })
            .ToListAsync(cancellationToken);

        model.CompanyOptions = companyOptions;
    }

    public async Task<UserManagementOperationResult> CreateAsync(
        SubsidiaryUserCreateViewModel model,
        CancellationToken cancellationToken = default)
    {
        string email = model.Email.Trim();
        string fullName = model.FullName.Trim();

        if (!model.CompanyId.HasValue)
        {
            return UserManagementOperationResult.Failure("Company is required.");
        }

        bool companyExists = await _dbContext.CompanyMasters
            .AnyAsync(
                company => company.Id == model.CompanyId.Value && company.IsActive,
                cancellationToken);

        if (!companyExists)
        {
            return UserManagementOperationResult.Failure(
                "Please select a valid active company.");
        }

        ApplicationUser? existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            return UserManagementOperationResult.Failure(
                "A user with this email already exists.");
        }

        ApplicationUser user = new()
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            CompanyId = model.CompanyId.Value,
            CreatedAtUtc = DateTime.UtcNow
        };

        ApplyActiveStatus(user, model.IsActive);

        IdentityResult createResult = await _userManager.CreateAsync(user, model.Password);

        if (!createResult.Succeeded)
        {
            return UserManagementOperationResult.Failure(
                FormatIdentityErrors(createResult.Errors));
        }

        IdentityResult roleResult = await _userManager.AddToRoleAsync(user, AppRoles.User);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return UserManagementOperationResult.Failure(
                FormatIdentityErrors(roleResult.Errors));
        }

        return UserManagementOperationResult.Success();
    }

    public async Task<SubsidiaryUserEditViewModel?> GetForEditAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return null;
        }

        bool isSubsidiaryUser = await _userManager.IsInRoleAsync(user, AppRoles.User);

        if (!isSubsidiaryUser)
        {
            return null;
        }

        SubsidiaryUserEditViewModel model = new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            CompanyId = user.CompanyId,
            IsActive = user.IsActive
        };

        await PopulateCompanyOptionsAsync(model, cancellationToken);

        return model;
    }

    public async Task<UserManagementOperationResult> UpdateAsync(
        string id,
        SubsidiaryUserEditViewModel model,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return UserManagementOperationResult.Failure("User was not found.");
        }

        bool isSubsidiaryUser = await _userManager.IsInRoleAsync(user, AppRoles.User);

        if (!isSubsidiaryUser)
        {
            return UserManagementOperationResult.Failure(
                "Only subsidiary users can be managed from this screen.");
        }

        string email = model.Email.Trim();
        string fullName = model.FullName.Trim();

        if (!model.CompanyId.HasValue)
        {
            return UserManagementOperationResult.Failure("Company is required.");
        }

        bool companyExists = await _dbContext.CompanyMasters
            .AnyAsync(
                company => company.Id == model.CompanyId.Value && company.IsActive,
                cancellationToken);

        if (!companyExists)
        {
            return UserManagementOperationResult.Failure(
                "Please select a valid active company.");
        }

        ApplicationUser? duplicateUser = await _userManager.FindByEmailAsync(email);

        if (duplicateUser is not null && duplicateUser.Id != user.Id)
        {
            return UserManagementOperationResult.Failure(
                "Another user with this email already exists.");
        }

        bool activeStatusChanged = user.IsActive != model.IsActive;

        user.FullName = fullName;
        user.Email = email;
        user.UserName = email;
        user.EmailConfirmed = true;
        user.CompanyId = model.CompanyId.Value;

        ApplyActiveStatus(user, model.IsActive);

        IdentityResult updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return UserManagementOperationResult.Failure(
                FormatIdentityErrors(updateResult.Errors));
        }

        if (activeStatusChanged)
        {
            IdentityResult stampResult = await _userManager.UpdateSecurityStampAsync(user);

            if (!stampResult.Succeeded)
            {
                return UserManagementOperationResult.Failure(
                    FormatIdentityErrors(stampResult.Errors));
            }
        }

        return UserManagementOperationResult.Success();
    }

    public async Task<UserManagementOperationResult> SetActiveStatusAsync(
        string id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return UserManagementOperationResult.Failure("User was not found.");
        }

        bool isSubsidiaryUser = await _userManager.IsInRoleAsync(user, AppRoles.User);

        if (!isSubsidiaryUser)
        {
            return UserManagementOperationResult.Failure(
                "Only subsidiary users can be managed from this screen.");
        }

        bool activeStatusChanged = user.IsActive != isActive;

        ApplyActiveStatus(user, isActive);

        IdentityResult updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return UserManagementOperationResult.Failure(
                FormatIdentityErrors(updateResult.Errors));
        }

        if (activeStatusChanged)
        {
            IdentityResult stampResult = await _userManager.UpdateSecurityStampAsync(user);

            if (!stampResult.Succeeded)
            {
                return UserManagementOperationResult.Failure(
                    FormatIdentityErrors(stampResult.Errors));
            }
        }

        return UserManagementOperationResult.Success();
    }

    private static void ApplyActiveStatus(ApplicationUser user, bool isActive)
    {
        user.IsActive = isActive;
        user.LockoutEnabled = true;
        user.LockoutEnd = isActive ? null : DateTimeOffset.MaxValue;

        if (isActive)
        {
            user.AccessFailedCount = 0;
        }
    }

    private static string FormatIdentityErrors(IEnumerable<IdentityError> errors)
    {
        return string.Join("; ", errors.Select(error => error.Description));
    }
}