using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Financings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFinancingType",
                table: "PaymentTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "FinancingId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FinancingInstallmentId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Financings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CorrectionIndex = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RemainingDebt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastCorrectionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Financings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Financings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancingCorrections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    IndexValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CorrectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PreviousDebt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewDebt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinancingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancingCorrections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancingCorrections_Financings_FinancingId",
                        column: x => x.FinancingId,
                        principalTable: "Financings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancingInstallments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    InstallmentNumber = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmortizationAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinancingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancingInstallments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancingInstallments_Financings_FinancingId",
                        column: x => x.FinancingId,
                        principalTable: "Financings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_FinancingId",
                table: "Payments",
                column: "FinancingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_FinancingInstallmentId",
                table: "Payments",
                column: "FinancingInstallmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancingCorrections_FinancingId",
                table: "FinancingCorrections",
                column: "FinancingId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancingInstallments_FinancingId",
                table: "FinancingInstallments",
                column: "FinancingId");

            migrationBuilder.CreateIndex(
                name: "IX_Financings_UserId",
                table: "Financings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_FinancingInstallments_FinancingInstallmentId",
                table: "Payments",
                column: "FinancingInstallmentId",
                principalTable: "FinancingInstallments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Financings_FinancingId",
                table: "Payments",
                column: "FinancingId",
                principalTable: "Financings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_FinancingInstallments_FinancingInstallmentId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Financings_FinancingId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "FinancingCorrections");

            migrationBuilder.DropTable(
                name: "FinancingInstallments");

            migrationBuilder.DropTable(
                name: "Financings");

            migrationBuilder.DropIndex(
                name: "IX_Payments_FinancingId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_FinancingInstallmentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsFinancingType",
                table: "PaymentTypes");

            migrationBuilder.DropColumn(
                name: "FinancingId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "FinancingInstallmentId",
                table: "Payments");
        }
    }
}
