using Cashflow.Web.Constants;
using Cashflow.Web.Services;
using Cashflow.Web.ViewModels.PaymentRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashflow.Web.Controllers;

[Authorize(Policy = AppPolicies.UserOnly)]
public class PaymentRequestsController : Controller
{
    private readonly IPaymentRequestSubmissionService _paymentRequestSubmissionService;

    public PaymentRequestsController(
        IPaymentRequestSubmissionService paymentRequestSubmissionService)
    {
        _paymentRequestSubmissionService = paymentRequestSubmissionService;
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        PaymentRequestCreateViewModel model = new();

        await _paymentRequestSubmissionService.PopulateCreateFormOptionsAsync(
            model,
            cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        PaymentRequestCreateViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await _paymentRequestSubmissionService.PopulateCreateFormOptionsAsync(
                model,
                cancellationToken);

            return View(model);
        }

        PaymentRequestOperationResult result =
            await _paymentRequestSubmissionService.CreateAsync(
                model,
                User,
                cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Could not submit payment request.");

            await _paymentRequestSubmissionService.PopulateCreateFormOptionsAsync(
                model,
                cancellationToken);

            return View(model);
        }

        TempData["SuccessMessage"] =
            $"Payment request #{result.PaymentRequestId} submitted successfully.";

        return RedirectToAction(nameof(Create));
    }
}