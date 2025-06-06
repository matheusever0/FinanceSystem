using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equilibrium.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditCardsToPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreditCardId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreditCardId",
                table: "Payments",
                column: "CreditCardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_CreditCards_CreditCardId",
                table: "Payments",
                column: "CreditCardId",
                principalTable: "CreditCards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_CreditCards_CreditCardId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CreditCardId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CreditCardId",
                table: "Payments");
        }
    }
}
