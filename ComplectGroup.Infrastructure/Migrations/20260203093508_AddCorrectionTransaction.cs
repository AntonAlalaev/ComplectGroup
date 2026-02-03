using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplectGroup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCorrectionTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CorrectionTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CorrectionNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    OldPartId = table.Column<int>(type: "INTEGER", nullable: false),
                    NewPartId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    CorrectionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrectionTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorrectionTransactions_Parts_NewPartId",
                        column: x => x.NewPartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorrectionTransactions_Parts_OldPartId",
                        column: x => x.OldPartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorrectionTransactions_NewPartId",
                table: "CorrectionTransactions",
                column: "NewPartId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectionTransactions_OldPartId",
                table: "CorrectionTransactions",
                column: "OldPartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorrectionTransactions");
        }
    }
}
