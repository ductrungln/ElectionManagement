using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ElectionManagement.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElectionProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: false),
                    TenKhuVuc = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DonVi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TongCuTri = table.Column<int>(type: "int", nullable: false),
                    Gio7 = table.Column<int>(type: "int", nullable: false),
                    Gio9 = table.Column<int>(type: "int", nullable: false),
                    Gio11 = table.Column<int>(type: "int", nullable: false),
                    Gio14 = table.Column<int>(type: "int", nullable: false),
                    Gio16 = table.Column<int>(type: "int", nullable: false),
                    Gio19 = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionProgresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElectionResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: false),
                    KhuVuc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TongCuTri = table.Column<int>(type: "int", nullable: false),
                    PhieuPhatRa = table.Column<int>(type: "int", nullable: false),
                    PhieuThuVe = table.Column<int>(type: "int", nullable: false),
                    PhieuHopLe = table.Column<int>(type: "int", nullable: false),
                    PhieuKhongHopLe = table.Column<int>(type: "int", nullable: false),
                    UngCuVien1 = table.Column<int>(type: "int", nullable: false),
                    UngCuVien2 = table.Column<int>(type: "int", nullable: false),
                    UngCuVien3 = table.Column<int>(type: "int", nullable: false),
                    UngCuVien4 = table.Column<int>(type: "int", nullable: false),
                    UngCuVien5 = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    SuccessRows = table.Column<int>(type: "int", nullable: false),
                    ErrorRows = table.Column<int>(type: "int", nullable: false),
                    Errors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElectionProgresses_Stt_TenKhuVuc",
                table: "ElectionProgresses",
                columns: new[] { "Stt", "TenKhuVuc" },
                unique: true,
                filter: "[TenKhuVuc] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionResults_Stt",
                table: "ElectionResults",
                column: "Stt",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
