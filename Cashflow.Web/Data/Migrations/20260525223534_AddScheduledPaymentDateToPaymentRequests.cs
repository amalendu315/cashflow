using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cashflow.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledPaymentDateToPaymentRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduledPaymentDateUtc",
                table: "PaymentRequests");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ScheduledPaymentDate",
                table: "PaymentRequests",
                type: "date",
                nullable: false,
                defaultValueSql: "CONVERT(date, DATEADD(MINUTE, 330, SYSUTCDATETIME()))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduledPaymentDate",
                table: "PaymentRequests");

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledPaymentDateUtc",
                table: "PaymentRequests",
                type: "datetime2",
                nullable: true);
        }
    }
}
