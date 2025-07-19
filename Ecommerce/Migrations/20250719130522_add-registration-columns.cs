using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Migrations
{
    /// <inheritdoc />
    public partial class addregistrationcolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Dealers",
                newName: "GSTNumber");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Dealers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "Dealers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirmName",
                table: "Dealers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "Contact",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "FirmName",
                table: "Dealers");

            migrationBuilder.RenameColumn(
                name: "GSTNumber",
                table: "Dealers",
                newName: "Location");
        }
    }
}
