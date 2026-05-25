using Cashflow.Web.Constants;
using Cashflow.Web.Services;
using Cashflow.Web.ViewModels.CompanyMasters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashflow.Web.Controllers;

[Authorize(Policy = AppPolicies.AdminOnly)]
public class CompanyMastersController : Controller
{
    private readonly ICompanyMasterService _companyMasterService;

    public CompanyMastersController(ICompanyMasterService companyMasterService)
    {
        _companyMasterService = companyMasterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        CompanyMasterIndexViewModel model =
            await _companyMasterService.GetIndexAsync(cancellationToken);

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CompanyMasterFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CompanyMasterFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        CompanyMasterOperationResult result =
            await _companyMasterService.CreateAsync(model, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not create company.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Company created successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        CompanyMasterFormViewModel? model =
            await _companyMasterService.GetForEditAsync(id, cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        CompanyMasterFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View(model);
        }

        CompanyMasterOperationResult result =
            await _companyMasterService.UpdateAsync(id, model, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not update company.");
            model.Id = id;
            return View(model);
        }

        TempData["SuccessMessage"] = "Company updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
    {
        CompanyMasterOperationResult result =
            await _companyMasterService.SetActiveStatusAsync(id, true, cancellationToken);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Company activated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Could not activate company.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        CompanyMasterOperationResult result =
            await _companyMasterService.SetActiveStatusAsync(id, false, cancellationToken);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Company deactivated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Could not deactivate company.";
        }

        return RedirectToAction(nameof(Index));
    }
}