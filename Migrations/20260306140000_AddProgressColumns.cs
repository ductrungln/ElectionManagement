using Microsoft.EntityFrameworkCore.Migrations;

namespace ElectionManagement.Migrations
{
    public partial class AddProgressColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm 2 cột mới vào bảng ElectionProgress
            migrationBuilder.RenameColumn(
                name: "Gio7",
                table: "ElectionProgress",
                newName: "Gio8");

            migrationBuilder.RenameColumn(
                name: "Gio9",
                table: "ElectionProgress",
                newName: "Gio10");

            migrationBuilder.DropColumn(
                name: "Gio11",
                table: "ElectionProgress");

            migrationBuilder.AddColumn<int>(
                name: "TongCuTriDiBau",
                table: "ElectionProgress",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TiLe",
                table: "ElectionProgress",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TongCuTriDiBau",
                table: "ElectionProgress");

            migrationBuilder.DropColumn(
                name: "TiLe",
                table: "ElectionProgress");

            migrationBuilder.RenameColumn(
                name: "Gio8",
                table: "ElectionProgress",
                newName: "Gio7");

            migrationBuilder.RenameColumn(
                name: "Gio10",
                table: "ElectionProgress",
                newName: "Gio9");

            migrationBuilder.AddColumn<int>(
                name: "Gio11",
                table: "ElectionProgress",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
