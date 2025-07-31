using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sonic.API.Migrations
{
    /// <inheritdoc />
    public partial class AddContactInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Contacts",
                table: "Venues",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Contacts",
                table: "Users",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Contacts",
                table: "Events",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Contacts",
                table: "Artists",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contacts",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "Contacts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Contacts",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Contacts",
                table: "Artists");
        }
    }
}
