using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kuafordeneme.Migrations
{
    /// <inheritdoc />
    public partial class AddKullaniciIDToMesajlar1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mesajlar_Kullanicilar_KullaniciID",
                table: "Mesajlar");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Mesajlar",
                table: "Mesajlar");

            migrationBuilder.RenameTable(
                name: "Mesajlar",
                newName: "Mesaj");

            migrationBuilder.RenameIndex(
                name: "IX_Mesajlar_KullaniciID",
                table: "Mesaj",
                newName: "IX_Mesaj_KullaniciID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Mesaj",
                table: "Mesaj",
                column: "MesajID");

            migrationBuilder.AddForeignKey(
                name: "FK_Mesaj_Kullanicilar_KullaniciID",
                table: "Mesaj",
                column: "KullaniciID",
                principalTable: "Kullanicilar",
                principalColumn: "KullaniciID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mesaj_Kullanicilar_KullaniciID",
                table: "Mesaj");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Mesaj",
                table: "Mesaj");

            migrationBuilder.RenameTable(
                name: "Mesaj",
                newName: "Mesajlar");

            migrationBuilder.RenameIndex(
                name: "IX_Mesaj_KullaniciID",
                table: "Mesajlar",
                newName: "IX_Mesajlar_KullaniciID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Mesajlar",
                table: "Mesajlar",
                column: "MesajID");

            migrationBuilder.AddForeignKey(
                name: "FK_Mesajlar_Kullanicilar_KullaniciID",
                table: "Mesajlar",
                column: "KullaniciID",
                principalTable: "Kullanicilar",
                principalColumn: "KullaniciID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
