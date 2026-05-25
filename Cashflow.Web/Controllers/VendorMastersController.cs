using Cashflow.Web.Constants;
using Cashflow.Web.Services;
using Cashflow.Web.ViewModels.VendorMasters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashflow.Web.Controllers;

[Authorize(Policy = AppPolicies.AdminOnly)]
public class VendorMastersController : Controller
{
    private readonly IVendorMasterService _vendorMasterService;

    public VendorMastersController(IVendorMasterService vendorMasterService)
    {
        _vendorMasterService = vendorMasterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        VendorMasterIndexViewModel model =
            await _vendorMasterService.GetIndexAsync(cancellationToken);

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new VendorMasterFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        VendorMasterFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        VendorMasterOperationResult result =
            await _vendorMasterService.CreateAsync(model, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not create vendor.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Vendor created successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        VendorMasterFormViewModel? model =
            await _vendorMasterService.GetForEditAsync(id, cancellationToken);

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
        VendorMasterFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View(model);
        }

        VendorMasterOperationResult result =
            await _vendorMasterService.UpdateAsync(id, model, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not update vendor.");
            model.Id = id;
            return View(model);
        }

        TempData["SuccessMessage"] = "Vendor updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
    {
        VendorMasterOperationResult result =
            await _vendorMasterService.SetActiveStatusAsync(id, true, cancellationToken);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Vendor activated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Could not activate vendor.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        VendorMasterOperationResult result =
            await _vendorMasterService.SetActiveStatusAsync(id, false, cancellationToken);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Vendor deactivated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Could not deactivate vendor.";
        }

        return RedirectToAction(nameof(Index));
    }
}