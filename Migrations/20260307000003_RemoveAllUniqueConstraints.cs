using Microsoft.EntityFrameworkCore.Migrations;

namespace ElectionManagement.Migrations
{
    public partial class RemoveAllUniqueConstraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop all unique constraints/indexes
            migrationBuilder.DropIndex(
                name: "IX_ElectionResults_Stt",
                table: "ElectionResults");

            migrationBuilder.DropIndex(
                name: "IX_ElectionProgresses_Stt_TenKhuVuc",
                table: "ElectionProgresses");

            // Create non-unique indexes
            migrationBuilder.CreateIndex(
                name: "IX_ElectionResults_Stt",
                table: "ElectionResults",
                column: "Stt",
                unique: false);

            migrationBuilder.CreateIndex(
                name: "IX_ElectionProgresses_Stt_TenKhuVuc",
                table: "ElectionProgresses",
                columns: new[] { "Stt", "TenKhuVuc" },
                unique: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ElectionResults_Stt",
                table: "ElectionResults");

            migrationBuilder.DropIndex(
                name: "IX_ElectionProgresses_Stt_TenKhuVuc",
                table: "ElectionProgresses");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionResults_Stt",
                table: "ElectionResults",
                column: "Stt",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectionProgresses_Stt_TenKhuVuc",
                table: "ElectionProgresses",
                columns: new[] { "Stt", "TenKhuVuc" },
                unique: true,
                filter: "[TenKhuVuc] IS NOT NULL");
        }
    }
}
