using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class init1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalVouchers_FinancialPeriodId",
                table: "JournalVouchers");

            migrationBuilder.DropIndex(
                name: "IX_JournalVouchers_JournalVoucherNumber_Id",
                table: "JournalVouchers");

            migrationBuilder.CreateIndex(
                name: "IX_JournalVouchers_FinancialPeriodId_JournalVoucherNumber",
                table: "JournalVouchers",
                columns: new[] { "FinancialPeriodId", "JournalVoucherNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalVouchers_FinancialPeriodId_JournalVoucherNumber",
                table: "JournalVouchers");

            migrationBuilder.CreateIndex(
                name: "IX_JournalVouchers_FinancialPeriodId",
                table: "JournalVouchers",
                column: "FinancialPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalVouchers_JournalVoucherNumber_Id",
                table: "JournalVouchers",
                columns: new[] { "JournalVoucherNumber", "Id" },
                unique: true);
        }
    }
}
