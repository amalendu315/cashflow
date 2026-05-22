namespace Cashflow.Web.Models.Entities;

public class CompanyMaster
{
    public int Id { get; set; }

    public string CompanyCode { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    public ICollection<PaymentRequest> PaymentRequests { get; set; } = new List<PaymentRequest>();
}