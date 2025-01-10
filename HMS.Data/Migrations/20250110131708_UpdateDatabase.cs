using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pictures",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pictures", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "AccommodationPackagePictures",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccommodationPackageID = table.Column<int>(type: "int", nullable: false),
                    PictureID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationPackagePictures", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AccommodationPackagePictures_AccommodationPackages_AccommodationPackageID",
                        column: x => x.AccommodationPackageID,
                        principalTable: "AccommodationPackages",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccommodationPackagePictures_Pictures_PictureID",
                        column: x => x.PictureID,
                        principalTable: "Pictures",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccommodationPictures",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccommodationID = table.Column<int>(type: "int", nullable: false),
                    PictureID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationPictures", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AccommodationPictures_Accommodations_AccommodationID",
                        column: x => x.AccommodationID,
                        principalTable: "Accommodations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccommodationPictures_Pictures_PictureID",
                        column: x => x.PictureID,
                        principalTable: "Pictures",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationPackagePictures_AccommodationPackageID",
                table: "AccommodationPackagePictures",
                column: "AccommodationPackageID");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationPackagePictures_PictureID",
                table: "AccommodationPackagePictures",
                column: "PictureID");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationPictures_AccommodationID",
                table: "AccommodationPictures",
                column: "AccommodationID");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationPictures_PictureID",
                table: "AccommodationPictures",
                column: "PictureID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccommodationPackagePictures");

            migrationBuilder.DropTable(
                name: "AccommodationPictures");

            migrationBuilder.DropTable(
                name: "Pictures");
        }
    }
}
