using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equilibrium.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCorrection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancingCorrections");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancingCorrections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    FinancingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IndexValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewDebt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousDebt = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_FinancingCorrections_FinancingId",
                table: "FinancingCorrections",
                column: "FinancingId");
        }
    }
}
