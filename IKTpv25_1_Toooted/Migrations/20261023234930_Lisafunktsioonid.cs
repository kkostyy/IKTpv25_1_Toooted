using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IKTpv25_1_Toooted.Migrations
{
    /// <inheritdoc />
    public partial class Lisafunktsioonid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Demo",
                table: "Toodetabel",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Margised",
                table: "Toodetabel",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Ostuhind",
                table: "Toodetabel",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Pant",
                table: "Toodetabel",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Pant",
                table: "OstuRead",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Soodus",
                table: "OstuRead",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TavaHind",
                table: "OstuRead",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OstisKasutajaId",
                table: "Ostunimekiri",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Demo",
                table: "Ostud",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Demo",
                table: "Kliendid",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PereKood",
                table: "Kasutajad",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Kampaaniad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nimi = table.Column<string>(type: "TEXT", nullable: false),
                    ToodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    SoodusProtsent = table.Column<int>(type: "INTEGER", nullable: false),
                    Algus = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Lopp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Automaatne = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rakendatud = table.Column<bool>(type: "INTEGER", nullable: false),
                    TavaHind = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kampaaniad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kampaaniad_Toodetabel_ToodeId",
                        column: x => x.ToodeId,
                        principalTable: "Toodetabel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Partiid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ToodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Kogus = table.Column<int>(type: "INTEGER", nullable: false),
                    Aegub = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Lisatud = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MahaKantud = table.Column<int>(type: "INTEGER", nullable: false),
                    MahaKantudAeg = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partiid", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Partiid_Toodetabel_ToodeId",
                        column: x => x.ToodeId,
                        principalTable: "Toodetabel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Saavutused",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KlientId = table.Column<int>(type: "INTEGER", nullable: false),
                    Kood = table.Column<string>(type: "TEXT", nullable: false),
                    Aeg = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Boonus = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Saavutused", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Saavutused_Kliendid_KlientId",
                        column: x => x.KlientId,
                        principalTable: "Kliendid",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0.10m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0.10m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0.10m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 51,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 52,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0.10m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0.10m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 56,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 58,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 59,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 60,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 61,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 62,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 63,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 64,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 65,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 66,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 67,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 68,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.UpdateData(
                table: "Toodetabel",
                keyColumn: "Id",
                keyValue: 69,
                columns: new[] { "Demo", "Margised", "Ostuhind", "Pant" },
                values: new object[] { false, null, null, 0m });

            migrationBuilder.CreateIndex(
                name: "IX_Kasutajad_PereKood",
                table: "Kasutajad",
                column: "PereKood");

            migrationBuilder.CreateIndex(
                name: "IX_Kampaaniad_ToodeId",
                table: "Kampaaniad",
                column: "ToodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Partiid_ToodeId",
                table: "Partiid",
                column: "ToodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Saavutused_KlientId_Kood",
                table: "Saavutused",
                columns: new[] { "KlientId", "Kood" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kampaaniad");

            migrationBuilder.DropTable(
                name: "Partiid");

            migrationBuilder.DropTable(
                name: "Saavutused");

            migrationBuilder.DropIndex(
                name: "IX_Kasutajad_PereKood",
                table: "Kasutajad");

            migrationBuilder.DropColumn(
                name: "Demo",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "Margised",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "Ostuhind",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "Pant",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "Pant",
                table: "OstuRead");

            migrationBuilder.DropColumn(
                name: "Soodus",
                table: "OstuRead");

            migrationBuilder.DropColumn(
                name: "TavaHind",
                table: "OstuRead");

            migrationBuilder.DropColumn(
                name: "OstisKasutajaId",
                table: "Ostunimekiri");

            migrationBuilder.DropColumn(
                name: "Demo",
                table: "Ostud");

            migrationBuilder.DropColumn(
                name: "Demo",
                table: "Kliendid");

            migrationBuilder.DropColumn(
                name: "PereKood",
                table: "Kasutajad");
        }
    }
}
