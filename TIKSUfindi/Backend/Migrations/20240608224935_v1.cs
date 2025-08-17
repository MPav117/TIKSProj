using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTemplate.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Gradovi",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City_ascii = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Admin_name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gradovi", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Identiteti",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tip = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identiteti", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Kalendari",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PocetniDatumi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KrajnjiDatumi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PocetniDatumiUgovora = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KrajnjiDatumiUgovora = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kalendari", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Korisnici",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Slika = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GradID = table.Column<int>(type: "int", nullable: false),
                    ProsecnaOcena = table.Column<float>(type: "real", nullable: true),
                    IdentitetID = table.Column<int>(type: "int", nullable: false),
                    Povezani = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Korisnici", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Korisnici_Gradovi_GradID",
                        column: x => x.GradID,
                        principalTable: "Gradovi",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Korisnici_Identiteti_IdentitetID",
                        column: x => x.IdentitetID,
                        principalTable: "Identiteti",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    ChatRoom = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Korisnici_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Korisnici",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Korisnici_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Korisnici",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Majstori",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ListaVestina = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KalendarID = table.Column<int>(type: "int", nullable: false),
                    Tip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GrupaID = table.Column<int>(type: "int", nullable: true),
                    VodjaGrupe = table.Column<int>(type: "int", nullable: false),
                    KorisnikID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Majstori", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Majstori_Kalendari_KalendarID",
                        column: x => x.KalendarID,
                        principalTable: "Kalendari",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Majstori_Korisnici_KorisnikID",
                        column: x => x.KorisnikID,
                        principalTable: "Korisnici",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Majstori_Majstori_GrupaID",
                        column: x => x.GrupaID,
                        principalTable: "Majstori",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Poslodavci",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Adresa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KorisnikID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Poslodavci", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Poslodavci_Korisnici_KorisnikID",
                        column: x => x.KorisnikID,
                        principalTable: "Korisnici",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZahteviZaGrupu",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prihvacen = table.Column<int>(type: "int", nullable: false),
                    MajstorPrimalacID = table.Column<int>(type: "int", nullable: false),
                    MajstorPosiljalacID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZahteviZaGrupu", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ZahteviZaGrupu_Majstori_MajstorPosiljalacID",
                        column: x => x.MajstorPosiljalacID,
                        principalTable: "Majstori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ZahteviZaGrupu_Majstori_MajstorPrimalacID",
                        column: x => x.MajstorPrimalacID,
                        principalTable: "Majstori",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Oglasi",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CenaPoSatu = table.Column<float>(type: "real", nullable: false),
                    ListaSlika = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ListaVestina = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Naslov = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumPostavljanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatumZavrsetka = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PoslodavacID = table.Column<int>(type: "int", nullable: false),
                    Prikazan = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oglasi", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Oglasi_Poslodavci_PoslodavacID",
                        column: x => x.PoslodavacID,
                        principalTable: "Poslodavci",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MajstoriOglasi",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MajstorId = table.Column<int>(type: "int", nullable: false),
                    OglasID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MajstoriOglasi", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MajstoriOglasi_Majstori_MajstorId",
                        column: x => x.MajstorId,
                        principalTable: "Majstori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_MajstoriOglasi_Oglasi_OglasID",
                        column: x => x.OglasID,
                        principalTable: "Oglasi",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ZahteviZaPosao",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prihvacen = table.Column<int>(type: "int", nullable: false),
                    CenaPoSatu = table.Column<float>(type: "real", nullable: false),
                    ListaSlika = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatumZavrsetka = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MajstorID = table.Column<int>(type: "int", nullable: false),
                    PoslodavacID = table.Column<int>(type: "int", nullable: false),
                    OglasID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZahteviZaPosao", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ZahteviZaPosao_Majstori_MajstorID",
                        column: x => x.MajstorID,
                        principalTable: "Majstori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ZahteviZaPosao_Oglasi_OglasID",
                        column: x => x.OglasID,
                        principalTable: "Oglasi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ZahteviZaPosao_Poslodavci_PoslodavacID",
                        column: x => x.PoslodavacID,
                        principalTable: "Poslodavci",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Ugovori",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MajstorID = table.Column<int>(type: "int", nullable: false),
                    PoslodavacID = table.Column<int>(type: "int", nullable: false),
                    ZahtevZaPosaoID = table.Column<int>(type: "int", nullable: false),
                    ImeMajstora = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImePoslodavca = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CenaPoSatu = table.Column<float>(type: "real", nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumPocetka = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatumZavrsetka = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PotpisMajstora = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PotpisPoslodavca = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ugovori", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Ugovori_Majstori_MajstorID",
                        column: x => x.MajstorID,
                        principalTable: "Majstori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ugovori_Poslodavci_PoslodavacID",
                        column: x => x.PoslodavacID,
                        principalTable: "Poslodavci",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Ugovori_ZahteviZaPosao_ZahtevZaPosaoID",
                        column: x => x.ZahtevZaPosaoID,
                        principalTable: "ZahteviZaPosao",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Recenzije",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ocena = table.Column<int>(type: "int", nullable: false),
                    ListaSlika = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UgovorID = table.Column<int>(type: "int", nullable: false),
                    PrimalacID = table.Column<int>(type: "int", nullable: false),
                    DavalacID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recenzije", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Recenzije_Korisnici_DavalacID",
                        column: x => x.DavalacID,
                        principalTable: "Korisnici",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Recenzije_Korisnici_PrimalacID",
                        column: x => x.PrimalacID,
                        principalTable: "Korisnici",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Recenzije_Ugovori_UgovorID",
                        column: x => x.UgovorID,
                        principalTable: "Ugovori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ReceiverId",
                table: "ChatMessages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId",
                table: "ChatMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_GradID",
                table: "Korisnici",
                column: "GradID");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_IdentitetID",
                table: "Korisnici",
                column: "IdentitetID");

            migrationBuilder.CreateIndex(
                name: "IX_Majstori_GrupaID",
                table: "Majstori",
                column: "GrupaID");

            migrationBuilder.CreateIndex(
                name: "IX_Majstori_KalendarID",
                table: "Majstori",
                column: "KalendarID");

            migrationBuilder.CreateIndex(
                name: "IX_Majstori_KorisnikID",
                table: "Majstori",
                column: "KorisnikID");

            migrationBuilder.CreateIndex(
                name: "IX_MajstoriOglasi_MajstorId",
                table: "MajstoriOglasi",
                column: "MajstorId");

            migrationBuilder.CreateIndex(
                name: "IX_MajstoriOglasi_OglasID",
                table: "MajstoriOglasi",
                column: "OglasID");

            migrationBuilder.CreateIndex(
                name: "IX_Oglasi_PoslodavacID",
                table: "Oglasi",
                column: "PoslodavacID");

            migrationBuilder.CreateIndex(
                name: "IX_Poslodavci_KorisnikID",
                table: "Poslodavci",
                column: "KorisnikID");

            migrationBuilder.CreateIndex(
                name: "IX_Recenzije_DavalacID",
                table: "Recenzije",
                column: "DavalacID");

            migrationBuilder.CreateIndex(
                name: "IX_Recenzije_PrimalacID",
                table: "Recenzije",
                column: "PrimalacID");

            migrationBuilder.CreateIndex(
                name: "IX_Recenzije_UgovorID",
                table: "Recenzije",
                column: "UgovorID");

            migrationBuilder.CreateIndex(
                name: "IX_Ugovori_MajstorID",
                table: "Ugovori",
                column: "MajstorID");

            migrationBuilder.CreateIndex(
                name: "IX_Ugovori_PoslodavacID",
                table: "Ugovori",
                column: "PoslodavacID");

            migrationBuilder.CreateIndex(
                name: "IX_Ugovori_ZahtevZaPosaoID",
                table: "Ugovori",
                column: "ZahtevZaPosaoID");

            migrationBuilder.CreateIndex(
                name: "IX_ZahteviZaGrupu_MajstorPosiljalacID",
                table: "ZahteviZaGrupu",
                column: "MajstorPosiljalacID");

            migrationBuilder.CreateIndex(
                name: "IX_ZahteviZaGrupu_MajstorPrimalacID",
                table: "ZahteviZaGrupu",
                column: "MajstorPrimalacID");

            migrationBuilder.CreateIndex(
                name: "IX_ZahteviZaPosao_MajstorID",
                table: "ZahteviZaPosao",
                column: "MajstorID");

            migrationBuilder.CreateIndex(
                name: "IX_ZahteviZaPosao_OglasID",
                table: "ZahteviZaPosao",
                column: "OglasID");

            migrationBuilder.CreateIndex(
                name: "IX_ZahteviZaPosao_PoslodavacID",
                table: "ZahteviZaPosao",
                column: "PoslodavacID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "MajstoriOglasi");

            migrationBuilder.DropTable(
                name: "Recenzije");

            migrationBuilder.DropTable(
                name: "ZahteviZaGrupu");

            migrationBuilder.DropTable(
                name: "Ugovori");

            migrationBuilder.DropTable(
                name: "ZahteviZaPosao");

            migrationBuilder.DropTable(
                name: "Majstori");

            migrationBuilder.DropTable(
                name: "Oglasi");

            migrationBuilder.DropTable(
                name: "Kalendari");

            migrationBuilder.DropTable(
                name: "Poslodavci");

            migrationBuilder.DropTable(
                name: "Korisnici");

            migrationBuilder.DropTable(
                name: "Gradovi");

            migrationBuilder.DropTable(
                name: "Identiteti");
        }
    }
}
