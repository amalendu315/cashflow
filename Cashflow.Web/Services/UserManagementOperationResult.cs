namespace Cashflow.Web.Services;

public sealed class UserManagementOperationResult
{
    private UserManagementOperationResult(bool succeeded, string? errorMessage)
    {
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }

    public string? ErrorMessage { get; }

    public static UserManagementOperationResult Success()
    {
        return new UserManagementOperationResult(true, null);
    }

    public static UserManagementOperationResult Failure(string errorMessage)
    {
        return new UserManagementOperationResult(false, errorMessage);
    }
}