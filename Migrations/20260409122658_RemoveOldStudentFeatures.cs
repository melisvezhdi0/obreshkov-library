using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObreshkovLibrary.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOldStudentFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookAvailabilityRequests");

            migrationBuilder.DropTable(
                name: "LoanExtensionRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookAvailabilityRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeactivatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    NotifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookAvailabilityRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookAvailabilityRequests_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookAvailabilityRequests_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoanExtensionRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanId = table.Column<int>(type: "int", nullable: false),
                    AdminResponseMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProcessedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestedDays = table.Column<int>(type: "int", nullable: false),
                    RequestedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanExtensionRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanExtensionRequests_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookAvailabilityRequests_BookId",
                table: "BookAvailabilityRequests",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookAvailabilityRequests_ClientId_BookId_IsActive",
                table: "BookAvailabilityRequests",
                columns: new[] { "ClientId", "BookId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_LoanExtensionRequests_LoanId",
                table: "LoanExtensionRequests",
                column: "LoanId",
                unique: true);
        }
    }
}
