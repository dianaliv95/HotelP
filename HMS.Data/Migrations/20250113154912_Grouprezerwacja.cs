using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class Grouprezerwacja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ChildrenCount",
                table: "GroupReservations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AdultCount",
                table: "GroupReservations",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "BreakfastAdults",
                table: "GroupReservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BreakfastChildren",
                table: "GroupReservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DinnerAdults",
                table: "GroupReservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DinnerChildren",
                table: "GroupReservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "GroupReservations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LunchAdults",
                table: "GroupReservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LunchChildren",
                table: "GroupReservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "GroupReservations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "GroupReservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakfastAdults",
                table: "GroupReservations");

            migrationBuilder.DropColumn(
                name: "BreakfastChildren",
                table: "GroupReservations");

            migrationBuilder.DropColumn(
                name: "DinnerAdults",
                table: "GroupReservations");

            migrationBuilder.DropColumn(
                name: "DinnerChildren",
                table: "GroupReservations");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "GroupReservations");

            migrationBuilder.DropColumn(
                name: "LunchAdults",
                table: "GroupReservations");

            migrationBuilder.DropColumn(
                name: "LunchChildren",
                table: "GroupReservations");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "GroupReservations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "GroupReservations");

            migrationBuilder.AlterColumn<int>(
                name: "ChildrenCount",
                table: "GroupReservations",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "AdultCount",
                table: "GroupReservations",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);
        }
    }
}
