namespace Cashflow.Web.Services;

public sealed class CompanyMasterOperationResult
{
    private CompanyMasterOperationResult(bool succeeded, string? errorMessage)
    {
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }

    public string? ErrorMessage { get; }

    public static CompanyMasterOperationResult Success()
    {
        return new CompanyMasterOperationResult(true, null);
    }

    public static CompanyMasterOperationResult Failure(string errorMessage)
    {
        return new CompanyMasterOperationResult(false, errorMessage);
    }
}