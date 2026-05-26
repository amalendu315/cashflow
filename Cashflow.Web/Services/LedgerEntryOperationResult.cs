namespace Cashflow.Web.Services;

public sealed class LedgerEntryOperationResult
{
    private LedgerEntryOperationResult(bool succeeded, string? errorMessage)
    {
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }

    public string? ErrorMessage { get; }

    public static LedgerEntryOperationResult Success()
    {
        return new LedgerEntryOperationResult(true, null);
    }

    public static LedgerEntryOperationResult Failure(string errorMessage)
    {
        return new LedgerEntryOperationResult(false, errorMessage);
    }
}