using Cashflow.Web.Models.Enums;
using System.Globalization;

namespace Cashflow.Web.ViewModels.PaymentRequests;

public class PaymentRequestIndexViewModel
{
    public string CompanyName { get; set; } = string.Empty;

    public string CompanyCode { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public IReadOnlyList<PaymentRequestListItemViewModel> Requests { get; set; }
        = new List<PaymentRequestListItemViewModel>();

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasRequests => Requests.Any();

    public int TotalCount => Requests.Count;

    public int PendingCount => Requests.Count(request => request.Status == PaymentRequestStatus.Pending);

    public int ApprovedCount => Requests.Count(request => request.Status == PaymentRequestStatus.Approved);

    public int SplitCount => Requests.Count(request => request.Status == PaymentRequestStatus.Split);

    public int RejectedCount => Requests.Count(request => request.Status == PaymentRequestStatus.Rejected);

    public int HighPriorityCount => Requests.Count(request => request.Priority == PaymentPriority.High);

    public decimal TotalRequestedAmount =>
        Requests.Sum(request => request.RequestedAmount);

    public decimal TotalPendingAmount =>
        Requests
            .Where(request => request.Status == PaymentRequestStatus.Pending)
            .Sum(request => request.RequestedAmount);

    public string CompanyDisplay =>
        string.IsNullOrWhiteSpace(CompanyCode)
            ? CompanyName
            : $"{CompanyName} ({CompanyCode})";

    public string TotalRequestedAmountDisplay =>
        "₹" + TotalRequestedAmount.ToString("N2", CultureInfo.InvariantCulture);

    public string TotalPendingAmountDisplay =>
        "₹" + TotalPendingAmount.ToString("N2", CultureInfo.InvariantCulture);
}