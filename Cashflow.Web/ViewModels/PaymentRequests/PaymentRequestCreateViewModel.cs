using Cashflow.Web.Models.Enums;
using Cashflow.Web.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Cashflow.Web.ViewModels.PaymentRequests;

public class PaymentRequestCreateViewModel
{
    [Required]
    [Display(Name = "Vendor")]
    public int? VendorMasterId { get; set; }

    [Required]
    [Display(Name = "Payment Priority")]
    public PaymentPriority? Priority { get; set; } = PaymentPriority.Normal;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Scheduled Payment Date")]
    public DateOnly? ScheduledPaymentDate { get; set; } = AppClock.TodayIst();

    [Required]
    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ErrorMessage = "Requested amount must be greater than zero.")]
    [DataType(DataType.Currency)]
    [Display(Name = "Requested Amount")]
    public decimal? RequestedAmount { get; set; }

    [StringLength(500)]
    [Display(Name = "Request Notes")]
    public string? RequestNotes { get; set; }

    public IReadOnlyList<SelectListItem> VendorOptions { get; set; }
        = new List<SelectListItem>();

    public IReadOnlyList<SelectListItem> PriorityOptions { get; set; }
        = new List<SelectListItem>();
}