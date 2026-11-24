using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplectGroup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlannedOperationActualQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActualQuantity",
                table: "PlannedOperations",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualQuantity",
                table: "PlannedOperations");
        }
    }
}
