using Microsoft.EntityFrameworkCore.Migrations;

namespace ElectionManagement.Migrations
{
    public partial class FixUniqueConstraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the unique constraint that's causing issues
            migrationBuilder.DropIndex(
                name: "IX_ElectionResults_Stt",
                table: "ElectionResults");

            // Create a non-unique index for performance (optional)
            migrationBuilder.CreateIndex(
                name: "IX_ElectionResults_Stt",
                table: "ElectionResults",
                column: "Stt",
                unique: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ElectionResults_Stt",
                table: "ElectionResults");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionResults_Stt",
                table: "ElectionResults",
                column: "Stt",
                unique: true);
        }
    }
}
