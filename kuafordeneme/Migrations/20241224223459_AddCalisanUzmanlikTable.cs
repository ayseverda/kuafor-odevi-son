using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kuafordeneme.Migrations
{
    /// <inheritdoc />
    public partial class AddCalisanUzmanlikTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalisanUzmanliklar",
                columns: table => new
                {
                    CalisanID = table.Column<int>(type: "integer", nullable: false),
                    IslemID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalisanUzmanliklar", x => new { x.CalisanID, x.IslemID });
                    table.ForeignKey(
                        name: "FK_CalisanUzmanliklar_Calisanlar_CalisanID",
                        column: x => x.CalisanID,
                        principalTable: "Calisanlar",
                        principalColumn: "CalisanID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalisanUzmanliklar_Islemler_IslemID",
                        column: x => x.IslemID,
                        principalTable: "Islemler",
                        principalColumn: "IslemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalisanUzmanliklar_IslemID",
                table: "CalisanUzmanliklar",
                column: "IslemID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalisanUzmanliklar");
        }
    }
}
