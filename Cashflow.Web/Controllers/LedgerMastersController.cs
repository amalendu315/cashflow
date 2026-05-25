using Cashflow.Web.Constants;
using Cashflow.Web.Services;
using Cashflow.Web.ViewModels.LedgerMasters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashflow.Web.Controllers;

[Authorize(Policy = AppPolicies.AdminOnly)]
public class LedgerMastersController : Controller
{
    private readonly ILedgerMasterService _ledgerMasterService;

    public LedgerMastersController(ILedgerMasterService ledgerMasterService)
    {
        _ledgerMasterService = ledgerMasterService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        LedgerMasterIndexViewModel model =
            await _ledgerMasterService.GetIndexAsync(cancellationToken);

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new LedgerMasterFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        LedgerMasterFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        LedgerMasterOperationResult result =
            await _ledgerMasterService.CreateAsync(model, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not create ledger.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Ledger created successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        LedgerMasterFormViewModel? model =
            await _ledgerMasterService.GetForEditAsync(id, cancellationToken);

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
        LedgerMasterFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View(model);
        }

        LedgerMasterOperationResult result =
            await _ledgerMasterService.UpdateAsync(id, model, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not update ledger.");
            model.Id = id;
            return View(model);
        }

        TempData["SuccessMessage"] = "Ledger updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
    {
        LedgerMasterOperationResult result =
            await _ledgerMasterService.SetActiveStatusAsync(id, true, cancellationToken);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Ledger activated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Could not activate ledger.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        LedgerMasterOperationResult result =
            await _ledgerMasterService.SetActiveStatusAsync(id, false, cancellationToken);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Ledger deactivated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Could not deactivate ledger.";
        }

        return RedirectToAction(nameof(Index));
    }
}