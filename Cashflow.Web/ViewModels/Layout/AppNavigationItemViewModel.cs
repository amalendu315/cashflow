namespace Cashflow.Web.ViewModels.Layout;

public class AppNavigationItemViewModel
{
    public string Label { get; set; } = string.Empty;

    public string IconCssClass { get; set; } = string.Empty;

    public string Controller { get; set; } = string.Empty;

    public string Action { get; set; } = "Index";

    public string Section { get; set; } = string.Empty;
}