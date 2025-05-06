using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equilibrium.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CurrencyInInvestment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Investments",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Investments");
        }
    }
}
