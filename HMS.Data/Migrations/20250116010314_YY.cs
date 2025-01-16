using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class YY : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BlockedFrom",
                table: "Rooms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BlockedTo",
                table: "Rooms",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockedFrom",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "BlockedTo",
                table: "Rooms");
        }
    }
}
