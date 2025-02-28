using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcomPlat.Data.Migrations
{
    /// <inheritdoc />
    public partial class BusinessAndWarehouseAddresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "ProductInventories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Warehouse",
                columns: table => new
                {
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StateRegion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CountryIso = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouse", x => x.WarehouseId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductInventories_WarehouseId",
                table: "ProductInventories",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductInventories_Warehouse_WarehouseId",
                table: "ProductInventories",
                column: "WarehouseId",
                principalTable: "Warehouse",
                principalColumn: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductInventories_Warehouse_WarehouseId",
                table: "ProductInventories");

            migrationBuilder.DropTable(
                name: "Warehouse");

            migrationBuilder.DropIndex(
                name: "IX_ProductInventories_WarehouseId",
                table: "ProductInventories");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "ProductInventories");
        }
    }
}
