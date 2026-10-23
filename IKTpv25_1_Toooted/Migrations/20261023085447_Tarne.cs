using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IKTpv25_1_Toooted.Migrations
{
    /// <inheritdoc />
    public partial class Tarne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Aadress",
                table: "Ostud",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Makseviis",
                table: "Ostud",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TarneHind",
                table: "Ostud",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Tarneviis",
                table: "Ostud",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Aadress",
                table: "Kasutajad",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TsekkEmailile",
                table: "Kasutajad",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aadress",
                table: "Ostud");

            migrationBuilder.DropColumn(
                name: "Makseviis",
                table: "Ostud");

            migrationBuilder.DropColumn(
                name: "TarneHind",
                table: "Ostud");

            migrationBuilder.DropColumn(
                name: "Tarneviis",
                table: "Ostud");

            migrationBuilder.DropColumn(
                name: "Aadress",
                table: "Kasutajad");

            migrationBuilder.DropColumn(
                name: "TsekkEmailile",
                table: "Kasutajad");
        }
    }
}
