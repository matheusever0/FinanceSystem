using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equilibrium.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCorrectionValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalCorrection",
                table: "FinancingInstallments",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalCorrection",
                table: "FinancingInstallments");
        }
    }
}
