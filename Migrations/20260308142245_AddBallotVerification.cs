using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ElectionManagement.Migrations
{
    public partial class AddBallotVerification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BallotVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistrictName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssuedBallots = table.Column<int>(type: "int", nullable: false),
                    ReceivedBallots = table.Column<int>(type: "int", nullable: false),
                    ValidBallots = table.Column<int>(type: "int", nullable: false),
                    InvalidBallots = table.Column<int>(type: "int", nullable: false),
                    BallotType1Count = table.Column<int>(type: "int", nullable: false),
                    BallotType1Votes = table.Column<int>(type: "int", nullable: false),
                    BallotType2Count = table.Column<int>(type: "int", nullable: false),
                    BallotType2Votes = table.Column<int>(type: "int", nullable: false),
                    BallotType3Count = table.Column<int>(type: "int", nullable: false),
                    BallotType3Votes = table.Column<int>(type: "int", nullable: false),
                    Candidate1Votes = table.Column<int>(type: "int", nullable: false),
                    Candidate2Votes = table.Column<int>(type: "int", nullable: false),
                    Candidate3Votes = table.Column<int>(type: "int", nullable: false),
                    Candidate4Votes = table.Column<int>(type: "int", nullable: false),
                    Candidate5Votes = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BallotVerifications", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BallotVerifications");
        }
    }
}
