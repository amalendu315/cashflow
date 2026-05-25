using Cashflow.Web.Constants;
using Cashflow.Web.Services;
using Cashflow.Web.ViewModels.AdminPaymentRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashflow.Web.Controllers;

[Authorize(Policy = AppPolicies.AdminOnly)]
public class AdminPaymentRequestsController : Controller
{
    private readonly IAdminPaymentRequestReviewService _reviewService;

    public AdminPaymentRequestsController(IAdminPaymentRequestReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        AdminPaymentRequestIndexViewModel model =
            await _reviewService.GetPendingIndexAsync(cancellationToken);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Review(long id, CancellationToken cancellationToken)
    {
        AdminPaymentRequestReviewViewModel? model =
            await _reviewService.GetForReviewAsync(id, cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveFull(
        long id,
        AdminPaymentRequestReviewViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            await _reviewService.PopulateLedgerOptionsAsync(model, cancellationToken);
            return View("Review", model);
        }

        AdminPaymentRequestReviewResult result =
            await _reviewService.ApproveFullAsync(id, model, User, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Could not approve request.");

            AdminPaymentRequestReviewViewModel? freshModel =
                await _reviewService.GetForReviewAsync(id, cancellationToken);

            if (freshModel is null)
            {
                return NotFound();
            }

            freshModel.ApprovedLedgerMasterId = model.ApprovedLedgerMasterId;
            freshModel.ReviewNotes = model.ReviewNotes;

            return View("Review", freshModel);
        }

        TempData["SuccessMessage"] = "Payment request approved successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(
        long id,
        AdminPaymentRequestReviewViewModel model,
        CancellationToken cancellationToken)
    {
        AdminPaymentRequestReviewResult result =
            await _reviewService.RejectAsync(id, model, User, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Could not reject request.");

            AdminPaymentRequestReviewViewModel? freshModel =
                await _reviewService.GetForReviewAsync(id, cancellationToken);

            if (freshModel is null)
            {
                return NotFound();
            }

            freshModel.ReviewNotes = model.ReviewNotes;

            return View("Review", freshModel);
        }

        TempData["SuccessMessage"] = "Payment request rejected successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Split(
        long id,
        AdminPaymentRequestReviewViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            await _reviewService.PopulateLedgerOptionsAsync(model, cancellationToken);
            return View("Review", model);
        }

        AdminPaymentRequestReviewResult result =
            await _reviewService.SplitAsync(id, model, User, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Could not split request.");

            AdminPaymentRequestReviewViewModel? freshModel =
                await _reviewService.GetForReviewAsync(id, cancellationToken);

            if (freshModel is null)
            {
                return NotFound();
            }

            freshModel.ApprovedLedgerMasterId = model.ApprovedLedgerMasterId;
            freshModel.SplitApprovedAmount = model.SplitApprovedAmount;
            freshModel.SplitRemainderScheduledPaymentDate =
                model.SplitRemainderScheduledPaymentDate;
            freshModel.ReviewNotes = model.ReviewNotes;

            return View("Review", freshModel);
        }

        TempData["SuccessMessage"] =
            result.ChildPaymentRequestId.HasValue
                ? $"Payment request split successfully. New pending request PR-{result.ChildPaymentRequestId.Value:D6} created."
                : "Payment request split successfully.";

        return RedirectToAction(nameof(Index));
    }
}