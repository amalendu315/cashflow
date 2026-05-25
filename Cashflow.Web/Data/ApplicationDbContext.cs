using Cashflow.Web.Models.Enums;
using Cashflow.Web.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CompanyMaster> CompanyMasters => Set<CompanyMaster>();

    public DbSet<LedgerMaster> LedgerMasters => Set<LedgerMaster>();

    public DbSet<VendorMaster> VendorMasters => Set<VendorMaster>();

    public DbSet<PaymentRequest> PaymentRequests => Set<PaymentRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FullName)
                .HasMaxLength(150);

            entity.Property(user => user.CreatedAtUtc)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(user => user.Company)
                .WithMany(company => company.Users)
                .HasForeignKey(user => user.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CompanyMaster>(entity =>
        {
            entity.ToTable("CompanyMasters");

            entity.HasKey(company => company.Id);

            entity.Property(company => company.CompanyCode)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(company => company.CompanyName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(company => company.IsActive)
                .HasDefaultValue(true);

            entity.Property(company => company.CreatedAtUtc)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(company => company.CompanyCode)
                .IsUnique();
        });

        builder.Entity<LedgerMaster>(entity =>
        {
            entity.ToTable("LedgerMasters");

            entity.HasKey(ledger => ledger.Id);

            entity.Property(ledger => ledger.LedgerCode)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(ledger => ledger.LedgerName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(ledger => ledger.Description)
                .HasMaxLength(300);

            entity.Property(ledger => ledger.IsActive)
                .HasDefaultValue(true);

            entity.Property(ledger => ledger.CreatedAtUtc)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(ledger => ledger.LedgerCode)
                .IsUnique();
        });

        builder.Entity<VendorMaster>(entity =>
        {
            entity.ToTable("VendorMasters");

            entity.HasKey(vendor => vendor.Id);

            entity.Property(vendor => vendor.VendorCode)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(vendor => vendor.VendorName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(vendor => vendor.GstNumber)
                .HasMaxLength(30);

            entity.Property(vendor => vendor.IsActive)
                .HasDefaultValue(true);

            entity.Property(vendor => vendor.CreatedAtUtc)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(vendor => vendor.VendorCode)
                .IsUnique();
        });

        builder.Entity<PaymentRequest>(entity =>
        {
            entity.ToTable("PaymentRequests");

            entity.HasKey(request => request.Id);

            entity.Property(request => request.RequestedAmount)
                .HasPrecision(18, 2);

            entity.Property(request => request.ApprovedAmount)
                .HasPrecision(18, 2);

            entity.Property(request => request.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(request => request.Priority)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue(PaymentPriority.Normal);

            entity.Property(request => request.ScheduledPaymentDate)
                .HasColumnType("date")
                .IsRequired()
                .HasDefaultValueSql("CONVERT(date, DATEADD(MINUTE, 330, SYSUTCDATETIME()))")
    .HasSentinel(DateOnly.MinValue);

            entity.Property(request => request.RequestNotes)
                .HasMaxLength(500);

            entity.Property(request => request.ReviewNotes)
                .HasMaxLength(500);

            entity.Property(request => request.RequestedByUserId)
                .IsRequired();

            entity.Property(request => request.RequestedAtUtc)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(request => request.Company)
                .WithMany(company => company.PaymentRequests)
                .HasForeignKey(request => request.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(request => request.VendorMaster)
                .WithMany(vendor => vendor.PaymentRequests)
                .HasForeignKey(request => request.VendorMasterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(request => request.ApprovedLedgerMaster)
                .WithMany(ledger => ledger.ApprovedPaymentRequests)
                .HasForeignKey(request => request.ApprovedLedgerMasterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(request => request.RequestedByUser)
                .WithMany()
                .HasForeignKey(request => request.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(request => request.ReviewedByUser)
                .WithMany()
                .HasForeignKey(request => request.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(request => request.ParentPaymentRequest)
                .WithMany(request => request.SplitChildren)
                .HasForeignKey(request => request.ParentPaymentRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(request => new { request.CompanyId, request.Status });

            entity.HasIndex(request => request.RequestedAtUtc);
        });
    }
}