using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcomPlat.Data.Migrations
{
    /// <inheritdoc />
    public partial class UniqueKeysOnCompaniesCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Subcategories_SubcategoryKey",
                table: "Subcategories",
                column: "SubcategoryKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                table: "Companies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategoryKey",
                table: "Categories",
                column: "CategoryKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subcategories_SubcategoryKey",
                table: "Subcategories");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Name",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CategoryKey",
                table: "Categories");
        }
    }
}
