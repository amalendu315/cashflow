using Cashflow.Web.Constants;
using Cashflow.Web.Services;
using Cashflow.Web.ViewModels.LedgerBalances;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashflow.Web.Controllers;

[Authorize(Policy = AppPolicies.AdminOnly)]
public class LedgerBalancesController : Controller
{
    private readonly ILedgerBalanceService _ledgerBalanceService;

    public LedgerBalancesController(ILedgerBalanceService ledgerBalanceService)
    {
        _ledgerBalanceService = ledgerBalanceService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int? ledgerMasterId,
        DateOnly? entryDate,
        CancellationToken cancellationToken)
    {
        LedgerBalanceIndexViewModel model =
            await _ledgerBalanceService.GetIndexAsync(
                ledgerMasterId,
                entryDate,
                cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOpeningBalance(
        OpeningBalanceFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please enter a valid opening balance.";
            return RedirectToAction(nameof(Index), new
            {
                ledgerMasterId = model.LedgerMasterId,
                entryDate = model.EntryDate
            });
        }

        LedgerEntryOperationResult result =
            await _ledgerBalanceService.SaveOpeningBalanceAsync(
                model,
                User,
                cancellationToken);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Opening balance saved successfully.";
        }
        else
        {
            TempData["ErrorMessage"] =
                result.ErrorMessage ?? "Could not save opening balance.";
        }

        return RedirectToAction(nameof(Index), new
        {
            ledgerMasterId = model.LedgerMasterId,
            entryDate = model.EntryDate
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddManualInflow(
        ManualInflowFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please enter a valid inflow.";
            return RedirectToAction(nameof(Index), new
            {
                ledgerMasterId = model.LedgerMasterId,
                entryDate = model.EntryDate
            });
        }

        LedgerEntryOperationResult result =
            await _ledgerBalanceService.AddManualInflowAsync(
                model,
                User,
                cancellationToken);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Manual inflow added successfully.";
        }
        else
        {
            TempData["ErrorMessage"] =
                result.ErrorMessage ?? "Could not add manual inflow.";
        }

        return RedirectToAction(nameof(Index), new
        {
            ledgerMasterId = model.LedgerMasterId,
            entryDate = model.EntryDate
        });
    }
}