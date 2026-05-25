using Cashflow.Web.Models.Enums;
using System.Globalization;

namespace Cashflow.Web.ViewModels.AdminPaymentRequests;

public class AdminPaymentRequestIndexViewModel
{
    public IReadOnlyList<AdminPaymentRequestListItemViewModel> PendingRequests { get; set; }
        = new List<AdminPaymentRequestListItemViewModel>();

    public int TotalPendingCount => PendingRequests.Count;

    public int HighPriorityCount =>
        PendingRequests.Count(request => request.Priority == PaymentPriority.High);

    public decimal TotalPendingAmount =>
        PendingRequests.Sum(request => request.RequestedAmount);

    public string TotalPendingAmountDisplay =>
        "₹" + TotalPendingAmount.ToString("N2", CultureInfo.InvariantCulture);

    public bool HasPendingRequests => PendingRequests.Any();
}