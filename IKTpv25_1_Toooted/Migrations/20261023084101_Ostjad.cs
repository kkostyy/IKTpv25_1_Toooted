using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IKTpv25_1_Toooted.Migrations
{
    /// <inheritdoc />
    public partial class Ostjad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Kasutajad",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KlientId",
                table: "Kasutajad",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nimi",
                table: "Kasutajad",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Ostunimekiri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KasutajaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tekst = table.Column<string>(type: "TEXT", nullable: false),
                    Kogus = table.Column<int>(type: "INTEGER", nullable: false),
                    ToodeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Ostetud = table.Column<bool>(type: "INTEGER", nullable: false),
                    Lisatud = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OstetudAeg = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ostunimekiri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ostunimekiri_Kasutajad_KasutajaId",
                        column: x => x.KasutajaId,
                        principalTable: "Kasutajad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ostunimekiri_Toodetabel_ToodeId",
                        column: x => x.ToodeId,
                        principalTable: "Toodetabel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kasutajad_KlientId",
                table: "Kasutajad",
                column: "KlientId");

            migrationBuilder.CreateIndex(
                name: "IX_Ostunimekiri_KasutajaId",
                table: "Ostunimekiri",
                column: "KasutajaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ostunimekiri_ToodeId",
                table: "Ostunimekiri",
                column: "ToodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Kasutajad_Kliendid_KlientId",
                table: "Kasutajad",
                column: "KlientId",
                principalTable: "Kliendid",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kasutajad_Kliendid_KlientId",
                table: "Kasutajad");

            migrationBuilder.DropTable(
                name: "Ostunimekiri");

            migrationBuilder.DropIndex(
                name: "IX_Kasutajad_KlientId",
                table: "Kasutajad");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Kasutajad");

            migrationBuilder.DropColumn(
                name: "KlientId",
                table: "Kasutajad");

            migrationBuilder.DropColumn(
                name: "Nimi",
                table: "Kasutajad");
        }
    }
}
