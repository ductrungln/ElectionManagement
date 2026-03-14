using Microsoft.EntityFrameworkCore.Migrations;

namespace ElectionManagement.Migrations
{
    public partial class AddBallotClassificationFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhieuBau01",
                table: "ElectionResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PhieuBau02",
                table: "ElectionResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PhieuBau03",
                table: "ElectionResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PhieuBau04",
                table: "ElectionResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhieuBau01",
                table: "ElectionResults");

            migrationBuilder.DropColumn(
                name: "PhieuBau02",
                table: "ElectionResults");

            migrationBuilder.DropColumn(
                name: "PhieuBau03",
                table: "ElectionResults");

            migrationBuilder.DropColumn(
                name: "PhieuBau04",
                table: "ElectionResults");
        }
    }
}
