using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class T : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccommodationPackageID1",
                table: "Accommodations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_AccommodationPackageID1",
                table: "Accommodations",
                column: "AccommodationPackageID1");

            migrationBuilder.AddForeignKey(
                name: "FK_Accommodations_AccommodationPackages_AccommodationPackageID1",
                table: "Accommodations",
                column: "AccommodationPackageID1",
                principalTable: "AccommodationPackages",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accommodations_AccommodationPackages_AccommodationPackageID1",
                table: "Accommodations");

            migrationBuilder.DropIndex(
                name: "IX_Accommodations_AccommodationPackageID1",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "AccommodationPackageID1",
                table: "Accommodations");
        }
    }
}
