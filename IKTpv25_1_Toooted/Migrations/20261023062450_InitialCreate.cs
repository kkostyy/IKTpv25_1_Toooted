using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IKTpv25_1_Toooted.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kasutajad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kasutajanimi = table.Column<string>(type: "TEXT", nullable: false),
                    ParoolHash = table.Column<string>(type: "TEXT", nullable: false),
                    Roll = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kasutajad", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kategooriatabel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kategooria_nimetus = table.Column<string>(type: "TEXT", nullable: false),
                    Kirjeldus = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategooriatabel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kliendid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nimi = table.Column<string>(type: "TEXT", nullable: false),
                    Kaardinumber = table.Column<string>(type: "TEXT", nullable: false),
                    Boonus = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kliendid", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Toodetabel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Toodenimetus = table.Column<string>(type: "TEXT", nullable: false),
                    Kogus = table.Column<int>(type: "INTEGER", nullable: false),
                    Hind = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Pilt = table.Column<string>(type: "TEXT", nullable: true),
                    KategooriaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Toodetabel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Toodetabel_Kategooriatabel_KategooriaId",
                        column: x => x.KategooriaId,
                        principalTable: "Kategooriatabel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ostud",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Aeg = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Vahesumma = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Soodustus = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Summa = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    BoonusLisatud = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Kasutaja = table.Column<string>(type: "TEXT", nullable: false),
                    Makstud = table.Column<bool>(type: "INTEGER", nullable: false),
                    MakstudSumma = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Tagastatud = table.Column<bool>(type: "INTEGER", nullable: false),
                    KlientId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ostud", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ostud_Kliendid_KlientId",
                        column: x => x.KlientId,
                        principalTable: "Kliendid",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OstuRead",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OstId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToodeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Toodenimetus = table.Column<string>(type: "TEXT", nullable: false),
                    Kogus = table.Column<int>(type: "INTEGER", nullable: false),
                    Hind = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OstuRead", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OstuRead_Ostud_OstId",
                        column: x => x.OstId,
                        principalTable: "Ostud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OstuRead_Toodetabel_ToodeId",
                        column: x => x.ToodeId,
                        principalTable: "Toodetabel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Kategooriatabel",
                columns: new[] { "Id", "Kategooria_nimetus", "Kirjeldus" },
                values: new object[,]
                {
                    { 1, "Toidutooted", null },
                    { 2, "Joogid", null },
                    { 3, "Maiustused", null }
                });

            migrationBuilder.InsertData(
                table: "Toodetabel",
                columns: new[] { "Id", "Hind", "KategooriaId", "Kogus", "Pilt", "Toodenimetus" },
                values: new object[,]
                {
                    { 1, 1.29m, 1, 50, "rukkileib.png", "Rukkileib" },
                    { 2, 1.89m, 1, 40, "kama.png", "Kama" },
                    { 3, 1.35m, 1, 60, "kilu.png", "Kilu tomatikastmes" },
                    { 4, 3.49m, 1, 25, "verivorst.png", "Verivorst" },
                    { 5, 1.19m, 2, 80, "kali.png", "Kali" },
                    { 6, 1.49m, 2, 100, "alecoq.png", "A. Le Coq Premium" },
                    { 7, 12.99m, 2, 20, "vanatallinn.png", "Vana Tallinn" },
                    { 8, 1.99m, 3, 70, "kalev_sokolaad.png", "Kalev šokolaad" },
                    { 9, 0.89m, 3, 90, "kohuke.png", "Kohuke" },
                    { 10, 0.99m, 3, 60, "mesikapp.png", "Kalev Mesikäpp" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kasutajad_Kasutajanimi",
                table: "Kasutajad",
                column: "Kasutajanimi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kliendid_Kaardinumber",
                table: "Kliendid",
                column: "Kaardinumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ostud_KlientId",
                table: "Ostud",
                column: "KlientId");

            migrationBuilder.CreateIndex(
                name: "IX_OstuRead_OstId",
                table: "OstuRead",
                column: "OstId");

            migrationBuilder.CreateIndex(
                name: "IX_OstuRead_ToodeId",
                table: "OstuRead",
                column: "ToodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Toodetabel_KategooriaId",
                table: "Toodetabel",
                column: "KategooriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kasutajad");

            migrationBuilder.DropTable(
                name: "OstuRead");

            migrationBuilder.DropTable(
                name: "Ostud");

            migrationBuilder.DropTable(
                name: "Toodetabel");

            migrationBuilder.DropTable(
                name: "Kliendid");

            migrationBuilder.DropTable(
                name: "Kategooriatabel");
        }
    }
}
