using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SzervizApp.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class TeljesSzervizAlapok : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Aktiv",
                table: "Ugyfelek",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Cim",
                table: "Ugyfelek",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Letrehozva",
                table: "Ugyfelek",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "FelelosFelhasznaloId",
                table: "Javitasok",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FelvevoFelhasznaloId",
                table: "Javitasok",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Garancialis",
                table: "Javitasok",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "KoltsegekVeglegesitve",
                table: "Javitasok",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Megjegyzes",
                table: "Javitasok",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TervezettBefejezes",
                table: "Javitasok",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Verzio",
                table: "Javitasok",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "Letrehozva",
                table: "Eszkozok",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Megjegyzes",
                table: "Eszkozok",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VasarlasDatuma",
                table: "Eszkozok",
                type: "TEXT",
                nullable: true);

            // SQLite feltöltött táblán nem fogad el CURRENT_TIMESTAMP alapértéket az ADD COLUMN-hoz.
            // A régi rekordok eredeti rögzítési ideje nem ismert: ezek a frissítés időpontját kapják.
            // Új rekordnál az entitás DateTime.UtcNow értéke kerül mentésre.
            migrationBuilder.Sql("UPDATE Ugyfelek SET Letrehozva = CURRENT_TIMESTAMP;");
            migrationBuilder.Sql("UPDATE Eszkozok SET Letrehozva = CURRENT_TIMESTAMP;");

            migrationBuilder.CreateTable(
                name: "Alkatreszek",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Cikkszam = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Nev = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Gyarto = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Egyseg = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    AktualisEgysegar = table.Column<decimal>(type: "TEXT", nullable: false),
                    Aktiv = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alkatreszek", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jogosultsagok",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kod = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Nev = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Leiras = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogosultsagok", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Felhasznalok",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Felhasznalonev = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    NormalizaltFelhasznalonev = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    JelszoHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Nev = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: true),
                    Telefon = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Aktiv = table.Column<bool>(type: "INTEGER", nullable: false),
                    JogosultsagId = table.Column<int>(type: "INTEGER", nullable: false),
                    UgyfelId = table.Column<int>(type: "INTEGER", nullable: true),
                    BelepesVerzio = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Felhasznalok", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Felhasznalok_Jogosultsagok_JogosultsagId",
                        column: x => x.JogosultsagId,
                        principalTable: "Jogosultsagok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Felhasznalok_Ugyfelek_UgyfelId",
                        column: x => x.UgyfelId,
                        principalTable: "Ugyfelek",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlkatreszFelhasznalasok",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JavitasId = table.Column<int>(type: "INTEGER", nullable: false),
                    AlkatreszId = table.Column<int>(type: "INTEGER", nullable: false),
                    RogzitoFelhasznaloId = table.Column<int>(type: "INTEGER", nullable: false),
                    Mennyiseg = table.Column<decimal>(type: "TEXT", nullable: false),
                    Egysegar = table.Column<decimal>(type: "TEXT", nullable: false),
                    Datum = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlkatreszFelhasznalasok", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlkatreszFelhasznalasok_Alkatreszek_AlkatreszId",
                        column: x => x.AlkatreszId,
                        principalTable: "Alkatreszek",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlkatreszFelhasznalasok_Felhasznalok_RogzitoFelhasznaloId",
                        column: x => x.RogzitoFelhasznaloId,
                        principalTable: "Felhasznalok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlkatreszFelhasznalasok_Javitasok_JavitasId",
                        column: x => x.JavitasId,
                        principalTable: "Javitasok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bevizsgalasok",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JavitasId = table.Column<int>(type: "INTEGER", nullable: false),
                    Eredmeny = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    BecsultKoltseg = table.Column<decimal>(type: "TEXT", nullable: true),
                    Datum = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VegezteFelhasznaloId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bevizsgalasok", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bevizsgalasok_Felhasznalok_VegezteFelhasznaloId",
                        column: x => x.VegezteFelhasznaloId,
                        principalTable: "Felhasznalok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bevizsgalasok_Javitasok_JavitasId",
                        column: x => x.JavitasId,
                        principalTable: "Javitasok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JavitasKoltsegek",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JavitasId = table.Column<int>(type: "INTEGER", nullable: false),
                    RogzitoFelhasznaloId = table.Column<int>(type: "INTEGER", nullable: false),
                    Megnevezes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Tipus = table.Column<int>(type: "INTEGER", nullable: false),
                    Osszeg = table.Column<decimal>(type: "TEXT", nullable: false),
                    Datum = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JavitasKoltsegek", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JavitasKoltsegek_Felhasznalok_RogzitoFelhasznaloId",
                        column: x => x.RogzitoFelhasznaloId,
                        principalTable: "Felhasznalok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JavitasKoltsegek_Javitasok_JavitasId",
                        column: x => x.JavitasId,
                        principalTable: "Javitasok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Munkanaplok",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JavitasId = table.Column<int>(type: "INTEGER", nullable: false),
                    FelhasznaloId = table.Column<int>(type: "INTEGER", nullable: false),
                    Leiras = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    Kezdes = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Befejezes = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LedolgozottOra = table.Column<decimal>(type: "TEXT", nullable: false),
                    Oradij = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Munkanaplok", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Munkanaplok_Felhasznalok_FelhasznaloId",
                        column: x => x.FelhasznaloId,
                        principalTable: "Felhasznalok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Munkanaplok_Javitasok_JavitasId",
                        column: x => x.JavitasId,
                        principalTable: "Javitasok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Uzenetek",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UgyfelId = table.Column<int>(type: "INTEGER", nullable: false),
                    JavitasId = table.Column<int>(type: "INTEGER", nullable: true),
                    FeladoFelhasznaloId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tartalom = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    KuldesDatuma = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uzenetek", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Uzenetek_Felhasznalok_FeladoFelhasznaloId",
                        column: x => x.FeladoFelhasznaloId,
                        principalTable: "Felhasznalok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Uzenetek_Javitasok_JavitasId",
                        column: x => x.JavitasId,
                        principalTable: "Javitasok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Uzenetek_Ugyfelek_UgyfelId",
                        column: x => x.UgyfelId,
                        principalTable: "Ugyfelek",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Visszaadasok",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JavitasId = table.Column<int>(type: "INTEGER", nullable: false),
                    RogzitoFelhasznaloId = table.Column<int>(type: "INTEGER", nullable: false),
                    VisszaadasDatuma = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtvevoNev = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Megjegyzes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visszaadasok", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visszaadasok_Felhasznalok_RogzitoFelhasznaloId",
                        column: x => x.RogzitoFelhasznaloId,
                        principalTable: "Felhasznalok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visszaadasok_Javitasok_JavitasId",
                        column: x => x.JavitasId,
                        principalTable: "Javitasok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Jogosultsagok",
                columns: new[] { "Id", "Kod", "Leiras", "Nev" },
                values: new object[,]
                {
                    { 1, "Recepcios", null, "Recepciós" },
                    { 2, "Szerelo", null, "Szerelő" },
                    { 3, "Manager", null, "Manager" },
                    { 4, "Ugyfel", null, "Ügyfél" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Javitasok_FelelosFelhasznaloId",
                table: "Javitasok",
                column: "FelelosFelhasznaloId");

            migrationBuilder.CreateIndex(
                name: "IX_Javitasok_FelvevoFelhasznaloId",
                table: "Javitasok",
                column: "FelvevoFelhasznaloId");

            migrationBuilder.CreateIndex(
                name: "IX_Alkatreszek_Cikkszam",
                table: "Alkatreszek",
                column: "Cikkszam",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlkatreszFelhasznalasok_AlkatreszId",
                table: "AlkatreszFelhasznalasok",
                column: "AlkatreszId");

            migrationBuilder.CreateIndex(
                name: "IX_AlkatreszFelhasznalasok_JavitasId",
                table: "AlkatreszFelhasznalasok",
                column: "JavitasId");

            migrationBuilder.CreateIndex(
                name: "IX_AlkatreszFelhasznalasok_RogzitoFelhasznaloId",
                table: "AlkatreszFelhasznalasok",
                column: "RogzitoFelhasznaloId");

            migrationBuilder.CreateIndex(
                name: "IX_Bevizsgalasok_JavitasId",
                table: "Bevizsgalasok",
                column: "JavitasId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bevizsgalasok_VegezteFelhasznaloId",
                table: "Bevizsgalasok",
                column: "VegezteFelhasznaloId");

            migrationBuilder.CreateIndex(
                name: "IX_Felhasznalok_JogosultsagId",
                table: "Felhasznalok",
                column: "JogosultsagId");

            migrationBuilder.CreateIndex(
                name: "IX_Felhasznalok_NormalizaltFelhasznalonev",
                table: "Felhasznalok",
                column: "NormalizaltFelhasznalonev",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Felhasznalok_UgyfelId",
                table: "Felhasznalok",
                column: "UgyfelId");

            migrationBuilder.CreateIndex(
                name: "IX_JavitasKoltsegek_JavitasId",
                table: "JavitasKoltsegek",
                column: "JavitasId");

            migrationBuilder.CreateIndex(
                name: "IX_JavitasKoltsegek_RogzitoFelhasznaloId",
                table: "JavitasKoltsegek",
                column: "RogzitoFelhasznaloId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogosultsagok_Kod",
                table: "Jogosultsagok",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Munkanaplok_FelhasznaloId",
                table: "Munkanaplok",
                column: "FelhasznaloId");

            migrationBuilder.CreateIndex(
                name: "IX_Munkanaplok_JavitasId",
                table: "Munkanaplok",
                column: "JavitasId");

            migrationBuilder.CreateIndex(
                name: "IX_Uzenetek_FeladoFelhasznaloId",
                table: "Uzenetek",
                column: "FeladoFelhasznaloId");

            migrationBuilder.CreateIndex(
                name: "IX_Uzenetek_JavitasId",
                table: "Uzenetek",
                column: "JavitasId");

            migrationBuilder.CreateIndex(
                name: "IX_Uzenetek_UgyfelId",
                table: "Uzenetek",
                column: "UgyfelId");

            migrationBuilder.CreateIndex(
                name: "IX_Visszaadasok_JavitasId",
                table: "Visszaadasok",
                column: "JavitasId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visszaadasok_RogzitoFelhasznaloId",
                table: "Visszaadasok",
                column: "RogzitoFelhasznaloId");

            migrationBuilder.AddForeignKey(
                name: "FK_Javitasok_Felhasznalok_FelelosFelhasznaloId",
                table: "Javitasok",
                column: "FelelosFelhasznaloId",
                principalTable: "Felhasznalok",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Javitasok_Felhasznalok_FelvevoFelhasznaloId",
                table: "Javitasok",
                column: "FelvevoFelhasznaloId",
                principalTable: "Felhasznalok",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Javitasok_Felhasznalok_FelelosFelhasznaloId",
                table: "Javitasok");

            migrationBuilder.DropForeignKey(
                name: "FK_Javitasok_Felhasznalok_FelvevoFelhasznaloId",
                table: "Javitasok");

            migrationBuilder.DropTable(
                name: "AlkatreszFelhasznalasok");

            migrationBuilder.DropTable(
                name: "Bevizsgalasok");

            migrationBuilder.DropTable(
                name: "JavitasKoltsegek");

            migrationBuilder.DropTable(
                name: "Munkanaplok");

            migrationBuilder.DropTable(
                name: "Uzenetek");

            migrationBuilder.DropTable(
                name: "Visszaadasok");

            migrationBuilder.DropTable(
                name: "Alkatreszek");

            migrationBuilder.DropTable(
                name: "Felhasznalok");

            migrationBuilder.DropTable(
                name: "Jogosultsagok");

            migrationBuilder.DropIndex(
                name: "IX_Javitasok_FelelosFelhasznaloId",
                table: "Javitasok");

            migrationBuilder.DropIndex(
                name: "IX_Javitasok_FelvevoFelhasznaloId",
                table: "Javitasok");

            migrationBuilder.DropColumn(
                name: "Aktiv",
                table: "Ugyfelek");

            migrationBuilder.DropColumn(
                name: "Cim",
                table: "Ugyfelek");

            migrationBuilder.DropColumn(
                name: "Letrehozva",
                table: "Ugyfelek");

            migrationBuilder.DropColumn(
                name: "FelelosFelhasznaloId",
                table: "Javitasok");

            migrationBuilder.DropColumn(
                name: "FelvevoFelhasznaloId",
                table: "Javitasok");

            migrationBuilder.DropColumn(
                name: "Garancialis",
                table: "Javitasok");

            migrationBuilder.DropColumn(
                name: "KoltsegekVeglegesitve",
                table: "Javitasok");

            migrationBuilder.DropColumn(
                name: "Megjegyzes",
                table: "Javitasok");

            migrationBuilder.DropColumn(
                name: "TervezettBefejezes",
                table: "Javitasok");

            migrationBuilder.DropColumn(
                name: "Verzio",
                table: "Javitasok");

            migrationBuilder.DropColumn(
                name: "Letrehozva",
                table: "Eszkozok");

            migrationBuilder.DropColumn(
                name: "Megjegyzes",
                table: "Eszkozok");

            migrationBuilder.DropColumn(
                name: "VasarlasDatuma",
                table: "Eszkozok");
        }
    }
}
