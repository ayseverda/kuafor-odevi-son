using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kuafordeneme.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCalisanlar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calisanlar_Islemler_IslemID",
                table: "Calisanlar");

            migrationBuilder.AlterColumn<int>(
                name: "UzmanlikID",
                table: "Calisanlar",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "IslemID",
                table: "Calisanlar",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "GunlukKazanc",
                table: "Calisanlar",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddForeignKey(
                name: "FK_Calisanlar_Islemler_IslemID",
                table: "Calisanlar",
                column: "IslemID",
                principalTable: "Islemler",
                principalColumn: "IslemID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calisanlar_Islemler_IslemID",
                table: "Calisanlar");

            migrationBuilder.AlterColumn<int>(
                name: "UzmanlikID",
                table: "Calisanlar",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IslemID",
                table: "Calisanlar",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GunlukKazanc",
                table: "Calisanlar",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AddForeignKey(
                name: "FK_Calisanlar_Islemler_IslemID",
                table: "Calisanlar",
                column: "IslemID",
                principalTable: "Islemler",
                principalColumn: "IslemID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
