namespace Cashflow.Web.Models.Entities;

public class VendorMaster
{
    public int Id { get; set; }

    public string VendorCode { get; set; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;

    public string? GstNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<PaymentRequest> PaymentRequests { get; set; } = new List<PaymentRequest>();
}