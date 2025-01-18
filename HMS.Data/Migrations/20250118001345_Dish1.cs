using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class Dish1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DishPictures_Dishes_DishID1",
                table: "DishPictures");

            migrationBuilder.DropIndex(
                name: "IX_DishPictures_DishID1",
                table: "DishPictures");

            migrationBuilder.DropColumn(
                name: "DishID1",
                table: "DishPictures");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DishID1",
                table: "DishPictures",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DishPictures_DishID1",
                table: "DishPictures",
                column: "DishID1");

            migrationBuilder.AddForeignKey(
                name: "FK_DishPictures_Dishes_DishID1",
                table: "DishPictures",
                column: "DishID1",
                principalTable: "Dishes",
                principalColumn: "ID");
        }
    }
}
