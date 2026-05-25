namespace Cashflow.Web.Services;

public sealed class AdminPaymentRequestReviewResult
{
    private AdminPaymentRequestReviewResult(
        bool succeeded,
        string? errorMessage,
        long? childPaymentRequestId)
    {
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
        ChildPaymentRequestId = childPaymentRequestId;
    }

    public bool Succeeded { get; }

    public string? ErrorMessage { get; }

    public long? ChildPaymentRequestId { get; }

    public static AdminPaymentRequestReviewResult Success(long? childPaymentRequestId = null)
    {
        return new AdminPaymentRequestReviewResult(true, null, childPaymentRequestId);
    }

    public static AdminPaymentRequestReviewResult Failure(string errorMessage)
    {
        return new AdminPaymentRequestReviewResult(false, errorMessage, null);
    }
}