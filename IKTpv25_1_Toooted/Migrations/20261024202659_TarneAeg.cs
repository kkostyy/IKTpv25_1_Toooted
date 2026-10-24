using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IKTpv25_1_Toooted.Migrations
{
    /// <inheritdoc />
    public partial class TarneAeg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TarneAeg",
                table: "Ostud",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TarneAeg",
                table: "Ostud");
        }
    }
}
