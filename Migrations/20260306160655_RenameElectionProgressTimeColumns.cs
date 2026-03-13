using Microsoft.EntityFrameworkCore.Migrations;

namespace ElectionManagement.Migrations
{
    public partial class RenameElectionProgressTimeColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Gio9",
                table: "ElectionProgresses",
                newName: "Gio8");

            migrationBuilder.RenameColumn(
                name: "Gio7",
                table: "ElectionProgresses",
                newName: "Gio12");

            migrationBuilder.RenameColumn(
                name: "Gio11",
                table: "ElectionProgresses",
                newName: "Gio10");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Gio8",
                table: "ElectionProgresses",
                newName: "Gio9");

            migrationBuilder.RenameColumn(
                name: "Gio12",
                table: "ElectionProgresses",
                newName: "Gio7");

            migrationBuilder.RenameColumn(
                name: "Gio10",
                table: "ElectionProgresses",
                newName: "Gio11");
        }
    }
}
