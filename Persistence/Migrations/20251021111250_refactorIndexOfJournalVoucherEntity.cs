using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class refactorIndexOfJournalVoucherEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalVouchers_JournalVoucherNumber",
                table: "JournalVouchers");

            migrationBuilder.CreateIndex(
                name: "IX_JournalVouchers_JournalVoucherNumber_Id",
                table: "JournalVouchers",
                columns: new[] { "JournalVoucherNumber", "Id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalVouchers_JournalVoucherNumber_Id",
                table: "JournalVouchers");

            migrationBuilder.CreateIndex(
                name: "IX_JournalVouchers_JournalVoucherNumber",
                table: "JournalVouchers",
                column: "JournalVoucherNumber",
                unique: true);
        }
    }
}
