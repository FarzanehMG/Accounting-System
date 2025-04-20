using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AccountCode = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AccountDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneralLedgerAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AccountCode = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AccountDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    GroupAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralLedgerAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneralLedgerAccounts_GroupAccounts_GroupAccountId",
                        column: x => x.GroupAccountId,
                        principalTable: "GroupAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubLedgerAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AccountCode = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AccountDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    GeneralLedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubLedgerAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubLedgerAccounts_GeneralLedgerAccounts_GeneralLedgerAccountId",
                        column: x => x.GeneralLedgerAccountId,
                        principalTable: "GeneralLedgerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetailedAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AccountCode = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AccountDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SubLedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailedAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetailedAccounts_SubLedgerAccounts_SubLedgerAccountId",
                        column: x => x.SubLedgerAccountId,
                        principalTable: "SubLedgerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetailedAccounts_IsActive",
                table: "DetailedAccounts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DetailedAccounts_SubLedgerAccountId",
                table: "DetailedAccounts",
                column: "SubLedgerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgerAccounts_GroupAccountId",
                table: "GeneralLedgerAccounts",
                column: "GroupAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgerAccounts_IsActive",
                table: "GeneralLedgerAccounts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAccounts_IsActive",
                table: "GroupAccounts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SubLedgerAccounts_GeneralLedgerAccountId",
                table: "SubLedgerAccounts",
                column: "GeneralLedgerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SubLedgerAccounts_IsActive",
                table: "SubLedgerAccounts",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetailedAccounts");

            migrationBuilder.DropTable(
                name: "SubLedgerAccounts");

            migrationBuilder.DropTable(
                name: "GeneralLedgerAccounts");

            migrationBuilder.DropTable(
                name: "GroupAccounts");
        }
    }
}
