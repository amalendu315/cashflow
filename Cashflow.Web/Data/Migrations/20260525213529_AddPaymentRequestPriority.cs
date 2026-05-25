using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cashflow.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentRequestPriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "PaymentRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Normal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "PaymentRequests");
        }
    }
}
