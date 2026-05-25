using Cashflow.Web.Constants;
using Cashflow.Web.Services;
using Cashflow.Web.ViewModels.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashflow.Web.Controllers;

[Authorize(Policy = AppPolicies.AdminOnly)]
public class UserManagementController : Controller
{
    private readonly IUserManagementService _userManagementService;

    public UserManagementController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        SubsidiaryUserIndexViewModel model =
            await _userManagementService.GetIndexAsync(cancellationToken);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        SubsidiaryUserCreateViewModel model = new();

        await _userManagementService.PopulateCompanyOptionsAsync(model, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        SubsidiaryUserCreateViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await _userManagementService.PopulateCompanyOptionsAsync(model, cancellationToken);
            return View(model);
        }

        UserManagementOperationResult result =
            await _userManagementService.CreateAsync(model, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not create user.");
            await _userManagementService.PopulateCompanyOptionsAsync(model, cancellationToken);
            return View(model);
        }

        TempData["SuccessMessage"] = "Subsidiary user created successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        SubsidiaryUserEditViewModel? model =
            await _userManagementService.GetForEditAsync(id, cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        string id,
        SubsidiaryUserEditViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            await _userManagementService.PopulateCompanyOptionsAsync(model, cancellationToken);
            return View(model);
        }

        UserManagementOperationResult result =
            await _userManagementService.UpdateAsync(id, model, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not update user.");
            model.Id = id;
            await _userManagementService.PopulateCompanyOptionsAsync(model, cancellationToken);
            return View(model);
        }

        TempData["SuccessMessage"] = "Subsidiary user updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(string id, CancellationToken cancellationToken)
    {
        UserManagementOperationResult result =
            await _userManagementService.SetActiveStatusAsync(id, true, cancellationToken);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Subsidiary user activated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Could not activate user.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string id, CancellationToken cancellationToken)
    {
        UserManagementOperationResult result =
            await _userManagementService.SetActiveStatusAsync(id, false, cancellationToken);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Subsidiary user deactivated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Could not deactivate user.";
        }

        return RedirectToAction(nameof(Index));
    }
}