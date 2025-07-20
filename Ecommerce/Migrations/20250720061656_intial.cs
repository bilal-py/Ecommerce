using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecommerce.Migrations
{
    /// <inheritdoc />
    public partial class intial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IconText = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerName = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    Zip = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "Dealers",
                columns: table => new
                {
                    DealerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerName = table.Column<string>(type: "text", nullable: false),
                    Contact = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    GSTNumber = table.Column<string>(type: "text", nullable: true),
                    FirmName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dealers", x => x.DealerId);
                });

            migrationBuilder.CreateTable(
                name: "RegisteredRollNumber",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RollNumber = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredRollNumber", x => x.Id);
                    table.UniqueConstraint("AK_RegisteredRollNumber_RollNumber", x => x.RollNumber);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userName = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    DealerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Dealers_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Dealers",
                        principalColumn: "DealerId");
                });

            migrationBuilder.CreateTable(
                name: "Warranties",
                columns: table => new
                {
                    WarrantyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RollNumber = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WarrantyStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WarrantyEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VehicleYear = table.Column<string>(type: "text", nullable: false),
                    VehicleMake = table.Column<string>(type: "text", nullable: false),
                    VehicleModel = table.Column<string>(type: "text", nullable: false),
                    VehicleVIN = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegisteredRollNumberId = table.Column<Guid>(type: "uuid", nullable: true),
                    VehicleWrapFull = table.Column<bool>(type: "boolean", nullable: false),
                    FrontFull = table.Column<bool>(type: "boolean", nullable: false),
                    FrontLeadingEdge = table.Column<bool>(type: "boolean", nullable: false),
                    BumpersFront = table.Column<bool>(type: "boolean", nullable: false),
                    HoodLead = table.Column<bool>(type: "boolean", nullable: false),
                    Mirrors = table.Column<bool>(type: "boolean", nullable: false),
                    BumpersBack = table.Column<bool>(type: "boolean", nullable: false),
                    EdgeGuard = table.Column<bool>(type: "boolean", nullable: false),
                    Windshield = table.Column<bool>(type: "boolean", nullable: false),
                    FendersLead = table.Column<bool>(type: "boolean", nullable: false),
                    RoofFull = table.Column<bool>(type: "boolean", nullable: false),
                    HoodFull = table.Column<bool>(type: "boolean", nullable: false),
                    RoofLead = table.Column<bool>(type: "boolean", nullable: false),
                    Headlamps = table.Column<bool>(type: "boolean", nullable: false),
                    Trunk = table.Column<bool>(type: "boolean", nullable: false),
                    RegisteredRollNumbersId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warranties", x => x.WarrantyId);
                    table.ForeignKey(
                        name: "FK_Warranties_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Warranties_Dealers_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Dealers",
                        principalColumn: "DealerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Warranties_RegisteredRollNumber_RegisteredRollNumbersId",
                        column: x => x.RegisteredRollNumbersId,
                        principalTable: "RegisteredRollNumber",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Warranties_RegisteredRollNumber_RollNumber",
                        column: x => x.RollNumber,
                        principalTable: "RegisteredRollNumber",
                        principalColumn: "RollNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    CartId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.CartId);
                    table.ForeignKey(
                        name: "FK_Carts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DealerId",
                table: "Users",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_CustomerId",
                table: "Warranties",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_DealerId",
                table: "Warranties",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_RegisteredRollNumbersId",
                table: "Warranties",
                column: "RegisteredRollNumbersId");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_RollNumber",
                table: "Warranties",
                column: "RollNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Warranties");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "RegisteredRollNumber");

            migrationBuilder.DropTable(
                name: "Dealers");
        }
    }
}
