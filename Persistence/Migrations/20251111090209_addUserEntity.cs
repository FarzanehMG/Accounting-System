using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addUserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClosedBy",
                table: "FinancialPeriods",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedDate",
                table: "FinancialPeriods",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccountBalanceEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinancialPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalDebit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCredit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalanceEntity", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountBalanceEntity");

            migrationBuilder.DropColumn(
                name: "ClosedBy",
                table: "FinancialPeriods");

            migrationBuilder.DropColumn(
                name: "ClosedDate",
                table: "FinancialPeriods");
        }
    }
}
