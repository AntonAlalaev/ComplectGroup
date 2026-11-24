using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplectGroup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PeriodStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductionPlanItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductionPlanId = table.Column<int>(type: "INTEGER", nullable: false),
                    PartId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceRequirementId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPlanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionPlanItems_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionPlanItems_ProductionPlans_ProductionPlanId",
                        column: x => x.ProductionPlanId,
                        principalTable: "ProductionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlannedOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductionPlanItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    PartOperationId = table.Column<int>(type: "INTEGER", nullable: false),
                    BrigadeId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlannedHours = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    ActualHours = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SkipReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlannedOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlannedOperations_Brigades_BrigadeId",
                        column: x => x.BrigadeId,
                        principalTable: "Brigades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlannedOperations_PartOperations_PartOperationId",
                        column: x => x.PartOperationId,
                        principalTable: "PartOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlannedOperations_ProductionPlanItems_ProductionPlanItemId",
                        column: x => x.ProductionPlanItemId,
                        principalTable: "ProductionPlanItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlannedOperations_BrigadeId",
                table: "PlannedOperations",
                column: "BrigadeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlannedOperations_PartOperationId",
                table: "PlannedOperations",
                column: "PartOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_PlannedOperations_ProductionPlanItemId",
                table: "PlannedOperations",
                column: "ProductionPlanItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPlanItems_PartId",
                table: "ProductionPlanItems",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPlanItems_ProductionPlanId",
                table: "ProductionPlanItems",
                column: "ProductionPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlannedOperations");

            migrationBuilder.DropTable(
                name: "ProductionPlanItems");

            migrationBuilder.DropTable(
                name: "ProductionPlans");
        }
    }
}
