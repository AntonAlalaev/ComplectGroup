using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplectGroup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsIgnoredToComplectation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsIgnored",
                table: "Complectations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsIgnored",
                table: "Complectations");
        }
    }
}
