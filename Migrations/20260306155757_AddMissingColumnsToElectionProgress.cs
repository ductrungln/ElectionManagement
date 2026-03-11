using Microsoft.EntityFrameworkCore.Migrations;

namespace ElectionManagement.Migrations
{
    public partial class AddMissingColumnsToElectionProgress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TiLe",
                table: "ElectionProgresses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TongCuTriDiBau",
                table: "ElectionProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TiLe",
                table: "ElectionProgresses");

            migrationBuilder.DropColumn(
                name: "TongCuTriDiBau",
                table: "ElectionProgresses");
        }
    }
}
