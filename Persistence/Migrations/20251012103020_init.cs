using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinancialPeriodTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountDescription = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    JournalEntryType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JournalVouchers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JournalVoucherTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JournalVoucherNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransactionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    JournalVoucherState = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinancialPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalVouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalVouchers_FinancialPeriods_FinancialPeriodId",
                        column: x => x.FinancialPeriodId,
                        principalTable: "FinancialPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GeneralLedgerAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountDescription = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    JournalEntryType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GroupAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubLedgerAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountDescription = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    JournalEntryType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GeneralLedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetailedAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountDescription = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    JournalEntryType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubLedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JournalEntryNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JournalEntryType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    DetailedAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubLedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JournalVoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntries_DetailedAccounts_DetailedAccountId",
                        column: x => x.DetailedAccountId,
                        principalTable: "DetailedAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JournalEntries_JournalVouchers_JournalVoucherId",
                        column: x => x.JournalVoucherId,
                        principalTable: "JournalVouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntries_SubLedgerAccounts_SubLedgerAccountId",
                        column: x => x.SubLedgerAccountId,
                        principalTable: "SubLedgerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    JournalEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_JournalEntryId",
                table: "Attachments",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailedAccounts_AccountCode",
                table: "DetailedAccounts",
                column: "AccountCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetailedAccounts_AccountTitle",
                table: "DetailedAccounts",
                column: "AccountTitle");

            migrationBuilder.CreateIndex(
                name: "IX_DetailedAccounts_IsActive",
                table: "DetailedAccounts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DetailedAccounts_JournalEntryType",
                table: "DetailedAccounts",
                column: "JournalEntryType");

            migrationBuilder.CreateIndex(
                name: "IX_DetailedAccounts_SubLedgerAccountId",
                table: "DetailedAccounts",
                column: "SubLedgerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialPeriods_FinancialPeriodTitle",
                table: "FinancialPeriods",
                column: "FinancialPeriodTitle");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialPeriods_IsActive",
                table: "FinancialPeriods",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialPeriods_StartDate_EndDate",
                table: "FinancialPeriods",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgerAccounts_AccountCode",
                table: "GeneralLedgerAccounts",
                column: "AccountCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgerAccounts_AccountTitle",
                table: "GeneralLedgerAccounts",
                column: "AccountTitle");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgerAccounts_GroupAccountId",
                table: "GeneralLedgerAccounts",
                column: "GroupAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgerAccounts_IsActive",
                table: "GeneralLedgerAccounts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgerAccounts_JournalEntryType",
                table: "GeneralLedgerAccounts",
                column: "JournalEntryType");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAccounts_AccountCode",
                table: "GroupAccounts",
                column: "AccountCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupAccounts_AccountTitle",
                table: "GroupAccounts",
                column: "AccountTitle");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAccounts_IsActive",
                table: "GroupAccounts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAccounts_JournalEntryType",
                table: "GroupAccounts",
                column: "JournalEntryType");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_DetailedAccountId",
                table: "JournalEntries",
                column: "DetailedAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_JournalEntryNumber",
                table: "JournalEntries",
                column: "JournalEntryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_JournalEntryType",
                table: "JournalEntries",
                column: "JournalEntryType");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_JournalVoucherId",
                table: "JournalEntries",
                column: "JournalVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_SubLedgerAccountId",
                table: "JournalEntries",
                column: "SubLedgerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalVouchers_FinancialPeriodId",
                table: "JournalVouchers",
                column: "FinancialPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalVouchers_JournalVoucherNumber",
                table: "JournalVouchers",
                column: "JournalVoucherNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalVouchers_JournalVoucherState",
                table: "JournalVouchers",
                column: "JournalVoucherState");

            migrationBuilder.CreateIndex(
                name: "IX_JournalVouchers_ReferenceNumber",
                table: "JournalVouchers",
                column: "ReferenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_JournalVouchers_TransactionTime",
                table: "JournalVouchers",
                column: "TransactionTime");

            migrationBuilder.CreateIndex(
                name: "IX_SubLedgerAccounts_AccountCode",
                table: "SubLedgerAccounts",
                column: "AccountCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubLedgerAccounts_AccountTitle",
                table: "SubLedgerAccounts",
                column: "AccountTitle");

            migrationBuilder.CreateIndex(
                name: "IX_SubLedgerAccounts_GeneralLedgerAccountId",
                table: "SubLedgerAccounts",
                column: "GeneralLedgerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SubLedgerAccounts_IsActive",
                table: "SubLedgerAccounts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SubLedgerAccounts_JournalEntryType",
                table: "SubLedgerAccounts",
                column: "JournalEntryType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "DetailedAccounts");

            migrationBuilder.DropTable(
                name: "JournalVouchers");

            migrationBuilder.DropTable(
                name: "SubLedgerAccounts");

            migrationBuilder.DropTable(
                name: "FinancialPeriods");

            migrationBuilder.DropTable(
                name: "GeneralLedgerAccounts");

            migrationBuilder.DropTable(
                name: "GroupAccounts");
        }
    }
}
