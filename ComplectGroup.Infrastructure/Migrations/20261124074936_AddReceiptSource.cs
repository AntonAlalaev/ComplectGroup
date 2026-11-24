using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplectGroup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductionPlanId",
                table: "ReceiptTransactions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "ReceiptTransactions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptTransactions_ProductionPlanId",
                table: "ReceiptTransactions",
                column: "ProductionPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptTransactions_ProductionPlans_ProductionPlanId",
                table: "ReceiptTransactions",
                column: "ProductionPlanId",
                principalTable: "ProductionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptTransactions_ProductionPlans_ProductionPlanId",
                table: "ReceiptTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptTransactions_ProductionPlanId",
                table: "ReceiptTransactions");

            migrationBuilder.DropColumn(
                name: "ProductionPlanId",
                table: "ReceiptTransactions");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "ReceiptTransactions");
        }
    }
}
