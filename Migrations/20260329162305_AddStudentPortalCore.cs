using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObreshkovLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentPortalCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExtended",
                table: "Loans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BookAvailabilityRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                name: "ClientFavoriteBooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientFavoriteBooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientFavoriteBooks_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientFavoriteBooks_Clients_ClientId",
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
                    RequestedDays = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminResponseMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
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

            migrationBuilder.CreateTable(
                name: "LoanPersonalNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanPersonalNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanPersonalNotes_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: true),
                    LoanId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentNotifications_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentNotifications_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentNotifications_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "IX_ClientFavoriteBooks_BookId",
                table: "ClientFavoriteBooks",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientFavoriteBooks_ClientId_BookId",
                table: "ClientFavoriteBooks",
                columns: new[] { "ClientId", "BookId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanExtensionRequests_LoanId",
                table: "LoanExtensionRequests",
                column: "LoanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanPersonalNotes_LoanId",
                table: "LoanPersonalNotes",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentNotifications_BookId",
                table: "StudentNotifications",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentNotifications_ClientId",
                table: "StudentNotifications",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentNotifications_LoanId",
                table: "StudentNotifications",
                column: "LoanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookAvailabilityRequests");

            migrationBuilder.DropTable(
                name: "ClientFavoriteBooks");

            migrationBuilder.DropTable(
                name: "LoanExtensionRequests");

            migrationBuilder.DropTable(
                name: "LoanPersonalNotes");

            migrationBuilder.DropTable(
                name: "StudentNotifications");

            migrationBuilder.DropColumn(
                name: "IsExtended",
                table: "Loans");
        }
    }
}
