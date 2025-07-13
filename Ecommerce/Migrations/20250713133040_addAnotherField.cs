using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Migrations
{
    /// <inheritdoc />
    public partial class addAnotherField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FrontFull",
                table: "Warranties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FrontLeadingEdge",
                table: "Warranties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VehicleWrapFull",
                table: "Warranties",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FrontFull",
                table: "Warranties");

            migrationBuilder.DropColumn(
                name: "FrontLeadingEdge",
                table: "Warranties");

            migrationBuilder.DropColumn(
                name: "VehicleWrapFull",
                table: "Warranties");
        }
    }
}
