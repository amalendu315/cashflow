namespace Cashflow.Web.Services;

public sealed class VendorMasterOperationResult
{
    private VendorMasterOperationResult(bool succeeded, string? errorMessage)
    {
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }

    public string? ErrorMessage { get; }

    public static VendorMasterOperationResult Success()
    {
        return new VendorMasterOperationResult(true, null);
    }

    public static VendorMasterOperationResult Failure(string errorMessage)
    {
        return new VendorMasterOperationResult(false, errorMessage);
    }
}