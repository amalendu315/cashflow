namespace Cashflow.Web.Services;

public sealed class LedgerMasterOperationResult
{
    private LedgerMasterOperationResult(bool succeeded, string? errorMessage)
    {
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }

    public string? ErrorMessage { get; }

    public static LedgerMasterOperationResult Success()
    {
        return new LedgerMasterOperationResult(true, null);
    }

    public static LedgerMasterOperationResult Failure(string errorMessage)
    {
        return new LedgerMasterOperationResult(false, errorMessage);
    }
}