using Microsoft.EntityFrameworkCore.Migrations;

namespace ElectionManagement.Migrations
{
    public partial class AddBallotType4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BallotType4Count",
                table: "BallotVerifications",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BallotType4Votes",
                table: "BallotVerifications",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BallotType4Count",
                table: "BallotVerifications");

            migrationBuilder.DropColumn(
                name: "BallotType4Votes",
                table: "BallotVerifications");
        }
    }
}
