using Microsoft.AspNetCore.Identity;

namespace Cashflow.Web.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public int? CompanyId { get; set; }

    public CompanyMaster? Company { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}