using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ElectionManagement.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BallotVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Level = table.Column<string>(type: "TEXT", nullable: true),
                    DistrictName = table.Column<string>(type: "TEXT", nullable: true),
                    IssuedBallots = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceivedBallots = table.Column<int>(type: "INTEGER", nullable: false),
                    ValidBallots = table.Column<int>(type: "INTEGER", nullable: false),
                    InvalidBallots = table.Column<int>(type: "INTEGER", nullable: false),
                    BallotType1Count = table.Column<int>(type: "INTEGER", nullable: false),
                    BallotType1Votes = table.Column<int>(type: "INTEGER", nullable: false),
                    BallotType2Count = table.Column<int>(type: "INTEGER", nullable: false),
                    BallotType2Votes = table.Column<int>(type: "INTEGER", nullable: false),
                    BallotType3Count = table.Column<int>(type: "INTEGER", nullable: false),
                    BallotType3Votes = table.Column<int>(type: "INTEGER", nullable: false),
                    Candidate1Votes = table.Column<int>(type: "INTEGER", nullable: false),
                    Candidate2Votes = table.Column<int>(type: "INTEGER", nullable: false),
                    Candidate3Votes = table.Column<int>(type: "INTEGER", nullable: false),
                    Candidate4Votes = table.Column<int>(type: "INTEGER", nullable: false),
                    Candidate5Votes = table.Column<int>(type: "INTEGER", nullable: false),
                    Candidate6Votes = table.Column<int>(type: "INTEGER", nullable: false),
                    Candidate7Votes = table.Column<int>(type: "INTEGER", nullable: false),
                    Candidate1Name = table.Column<string>(type: "TEXT", nullable: true),
                    Candidate2Name = table.Column<string>(type: "TEXT", nullable: true),
                    Candidate3Name = table.Column<string>(type: "TEXT", nullable: true),
                    Candidate4Name = table.Column<string>(type: "TEXT", nullable: true),
                    Candidate5Name = table.Column<string>(type: "TEXT", nullable: true),
                    Candidate6Name = table.Column<string>(type: "TEXT", nullable: true),
                    Candidate7Name = table.Column<string>(type: "TEXT", nullable: true),
                    TotalCandidates = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BallotVerifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElectionProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Stt = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<string>(type: "TEXT", nullable: true),
                    TenKhuVuc = table.Column<string>(type: "TEXT", nullable: true),
                    DonVi = table.Column<string>(type: "TEXT", nullable: true),
                    TongCuTri = table.Column<int>(type: "INTEGER", nullable: false),
                    Gio8 = table.Column<int>(type: "INTEGER", nullable: false),
                    Gio10 = table.Column<int>(type: "INTEGER", nullable: false),
                    Gio12 = table.Column<int>(type: "INTEGER", nullable: false),
                    Gio14 = table.Column<int>(type: "INTEGER", nullable: false),
                    Gio16 = table.Column<int>(type: "INTEGER", nullable: false),
                    Gio19 = table.Column<int>(type: "INTEGER", nullable: false),
                    TongCuTriDiBau = table.Column<int>(type: "INTEGER", nullable: false),
                    TiLe = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionProgresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElectionResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Stt = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<string>(type: "TEXT", nullable: true),
                    KhuVuc = table.Column<string>(type: "TEXT", nullable: true),
                    TongCuTri = table.Column<int>(type: "INTEGER", nullable: false),
                    To = table.Column<string>(type: "TEXT", nullable: true),
                    PhieuPhatRa = table.Column<int>(type: "INTEGER", nullable: false),
                    PhieuThuVe = table.Column<int>(type: "INTEGER", nullable: false),
                    PhieuHopLe = table.Column<int>(type: "INTEGER", nullable: false),
                    PhieuKhongHopLe = table.Column<int>(type: "INTEGER", nullable: false),
                    UngCuVien1 = table.Column<int>(type: "INTEGER", nullable: false),
                    UngCuVien2 = table.Column<int>(type: "INTEGER", nullable: false),
                    UngCuVien3 = table.Column<int>(type: "INTEGER", nullable: false),
                    UngCuVien4 = table.Column<int>(type: "INTEGER", nullable: false),
                    UngCuVien5 = table.Column<int>(type: "INTEGER", nullable: false),
                    UngCuVien6 = table.Column<int>(type: "INTEGER", nullable: false),
                    UngCuVien7 = table.Column<int>(type: "INTEGER", nullable: false),
                    UngCuVien8 = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: true),
                    ImportType = table.Column<string>(type: "TEXT", nullable: true),
                    Level = table.Column<string>(type: "TEXT", nullable: true),
                    TotalRows = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessRows = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorRows = table.Column<int>(type: "INTEGER", nullable: false),
                    Errors = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: true),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    Role = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElectionProgresses_Stt_TenKhuVuc",
                table: "ElectionProgresses",
                columns: new[] { "Stt", "TenKhuVuc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BallotVerifications");

            migrationBuilder.DropTable(
                name: "ElectionProgresses");

            migrationBuilder.DropTable(
                name: "ElectionResults");

            migrationBuilder.DropTable(
                name: "ImportLogs");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
