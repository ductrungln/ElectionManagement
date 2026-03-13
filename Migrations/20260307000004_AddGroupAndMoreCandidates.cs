using Microsoft.EntityFrameworkCore.Migrations;

namespace ElectionManagement.Migrations
{
    public partial class AddGroupAndMoreCandidates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns to ElectionResults
            migrationBuilder.AddColumn<string>(
                name: "To",
                table: "ElectionResults",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UngCuVien6",
                table: "ElectionResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UngCuVien7",
                table: "ElectionResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UngCuVien8",
                table: "ElectionResults",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "To",
                table: "ElectionResults");

            migrationBuilder.DropColumn(
                name: "UngCuVien6",
                table: "ElectionResults");

            migrationBuilder.DropColumn(
                name: "UngCuVien7",
                table: "ElectionResults");

            migrationBuilder.DropColumn(
                name: "UngCuVien8",
                table: "ElectionResults");
        }
    }
}
