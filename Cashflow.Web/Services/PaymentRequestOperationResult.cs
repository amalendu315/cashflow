namespace Cashflow.Web.Services;

public sealed class PaymentRequestOperationResult
{
    private PaymentRequestOperationResult(
        bool succeeded,
        string? errorMessage,
        long? paymentRequestId)
    {
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
        PaymentRequestId = paymentRequestId;
    }

    public bool Succeeded { get; }

    public string? ErrorMessage { get; }

    public long? PaymentRequestId { get; }

    public static PaymentRequestOperationResult Success(long paymentRequestId)
    {
        return new PaymentRequestOperationResult(true, null, paymentRequestId);
    }

    public static PaymentRequestOperationResult Failure(string errorMessage)
    {
        return new PaymentRequestOperationResult(false, errorMessage, null);
    }
}