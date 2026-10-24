using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IKTpv25_1_Toooted.Migrations
{
    /// <inheritdoc />
    public partial class PereKorv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PereKorv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PereKood = table.Column<string>(type: "TEXT", nullable: false),
                    ToodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Kogus = table.Column<int>(type: "INTEGER", nullable: false),
                    KasutajaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Lisaja = table.Column<string>(type: "TEXT", nullable: false),
                    Aeg = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PereKorv", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PereKorv_Toodetabel_ToodeId",
                        column: x => x.ToodeId,
                        principalTable: "Toodetabel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PereKorv_PereKood",
                table: "PereKorv",
                column: "PereKood");

            migrationBuilder.CreateIndex(
                name: "IX_PereKorv_ToodeId",
                table: "PereKorv",
                column: "ToodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PereKorv");
        }
    }
}
