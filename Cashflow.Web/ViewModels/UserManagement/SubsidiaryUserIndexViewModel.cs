namespace Cashflow.Web.ViewModels.UserManagement;

public class SubsidiaryUserIndexViewModel
{
    public IReadOnlyList<SubsidiaryUserListItemViewModel> Users { get; set; }
        = new List<SubsidiaryUserListItemViewModel>();

    public int TotalCount => Users.Count;

    public int ActiveCount => Users.Count(user => user.IsActive);

    public int InactiveCount => Users.Count(user => !user.IsActive);
}