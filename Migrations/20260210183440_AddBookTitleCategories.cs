using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObreshkovLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddBookTitleCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImagePath",
                table: "BookTitles");

            migrationBuilder.RenameColumn(
                name: "PublishYear",
                table: "BookTitles",
                newName: "Year");

            migrationBuilder.AlterColumn<string>(
                name: "Section",
                table: "Clients",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Grade",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverUrl",
                table: "BookTitles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookTitleCategories",
                columns: table => new
                {
                    BookTitleId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookTitleCategories", x => new { x.BookTitleId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_BookTitleCategories_BookTitles_BookTitleId",
                        column: x => x.BookTitleId,
                        principalTable: "BookTitles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookTitleCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookTitleCategories_CategoryId",
                table: "BookTitleCategories",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookTitleCategories");

            migrationBuilder.DropColumn(
                name: "CoverUrl",
                table: "BookTitles");

            migrationBuilder.RenameColumn(
                name: "Year",
                table: "BookTitles",
                newName: "PublishYear");

            migrationBuilder.AlterColumn<string>(
                name: "Section",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<int>(
                name: "Grade",
                table: "Clients",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CoverImagePath",
                table: "BookTitles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
