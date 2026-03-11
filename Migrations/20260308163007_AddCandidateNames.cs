using Microsoft.EntityFrameworkCore.Migrations;

namespace ElectionManagement.Migrations
{
    public partial class AddCandidateNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Candidate1Name",
                table: "BallotVerifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Candidate2Name",
                table: "BallotVerifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Candidate3Name",
                table: "BallotVerifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Candidate4Name",
                table: "BallotVerifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Candidate5Name",
                table: "BallotVerifications",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Candidate1Name",
                table: "BallotVerifications");

            migrationBuilder.DropColumn(
                name: "Candidate2Name",
                table: "BallotVerifications");

            migrationBuilder.DropColumn(
                name: "Candidate3Name",
                table: "BallotVerifications");

            migrationBuilder.DropColumn(
                name: "Candidate4Name",
                table: "BallotVerifications");

            migrationBuilder.DropColumn(
                name: "Candidate5Name",
                table: "BallotVerifications");
        }
    }
}
