using Microsoft.EntityFrameworkCore.Migrations;

namespace ElectionManagement.Migrations
{
    public partial class RemoveSttUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ElectionResults_Stt",
                table: "ElectionResults");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ElectionResults_Stt",
                table: "ElectionResults",
                column: "Stt",
                unique: true);
        }
    }
}
