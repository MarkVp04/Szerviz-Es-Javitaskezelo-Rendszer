using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzervizApp.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ugyfelek",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nev = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Telefon = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ugyfelek", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Eszkozok",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TulajdonosId = table.Column<int>(type: "INTEGER", nullable: false),
                    EszkozTipus = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Gyarto = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Sorozatszam = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eszkozok", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Eszkozok_Ugyfelek_TulajdonosId",
                        column: x => x.TulajdonosId,
                        principalTable: "Ugyfelek",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Javitasok",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UgyfelId = table.Column<int>(type: "INTEGER", nullable: false),
                    EszkozId = table.Column<int>(type: "INTEGER", nullable: false),
                    BejelentettHiba = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    AtvetelDatuma = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Allapot = table.Column<int>(type: "INTEGER", nullable: false),
                    LezarasDatuma = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Eredmeny = table.Column<int>(type: "INTEGER", nullable: true),
                    LezarasIndoklasa = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Javitasok", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Javitasok_Eszkozok_EszkozId",
                        column: x => x.EszkozId,
                        principalTable: "Eszkozok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Javitasok_Ugyfelek_UgyfelId",
                        column: x => x.UgyfelId,
                        principalTable: "Ugyfelek",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Eszkozok_TulajdonosId",
                table: "Eszkozok",
                column: "TulajdonosId");

            migrationBuilder.CreateIndex(
                name: "IX_Javitasok_EszkozId",
                table: "Javitasok",
                column: "EszkozId");

            migrationBuilder.CreateIndex(
                name: "IX_Javitasok_UgyfelId",
                table: "Javitasok",
                column: "UgyfelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Javitasok");

            migrationBuilder.DropTable(
                name: "Eszkozok");

            migrationBuilder.DropTable(
                name: "Ugyfelek");
        }
    }
}
