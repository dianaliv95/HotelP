using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class Dish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DishPictures",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DishID = table.Column<int>(type: "int", nullable: false),
                    PictureID = table.Column<int>(type: "int", nullable: false),
                    DishID1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishPictures", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DishPictures_Dishes_DishID",
                        column: x => x.DishID,
                        principalTable: "Dishes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DishPictures_Dishes_DishID1",
                        column: x => x.DishID1,
                        principalTable: "Dishes",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DishPictures_Pictures_PictureID",
                        column: x => x.PictureID,
                        principalTable: "Pictures",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DishPictures_DishID",
                table: "DishPictures",
                column: "DishID");

            migrationBuilder.CreateIndex(
                name: "IX_DishPictures_DishID1",
                table: "DishPictures",
                column: "DishID1");

            migrationBuilder.CreateIndex(
                name: "IX_DishPictures_PictureID",
                table: "DishPictures",
                column: "PictureID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DishPictures");
        }
    }
}
