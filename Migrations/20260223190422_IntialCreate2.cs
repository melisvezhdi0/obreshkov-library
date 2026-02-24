using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObreshkovLibrary.Migrations
{
    /// <inheritdoc />
    public partial class IntialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookCopies_BookTitles_BookId",
                table: "BookCopies");

            migrationBuilder.DropForeignKey(
                name: "FK_BookTitles_Categories_CategoryId",
                table: "BookTitles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookTitles",
                table: "BookTitles");

            migrationBuilder.RenameTable(
                name: "BookTitles",
                newName: "Books");

            migrationBuilder.RenameIndex(
                name: "IX_BookTitles_CategoryId",
                table: "Books",
                newName: "IX_Books_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Books",
                table: "Books",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookCopies_Books_BookId",
                table: "BookCopies",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Categories_CategoryId",
                table: "Books",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookCopies_Books_BookId",
                table: "BookCopies");

            migrationBuilder.DropForeignKey(
                name: "FK_Books_Categories_CategoryId",
                table: "Books");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Books",
                table: "Books");

            migrationBuilder.RenameTable(
                name: "Books",
                newName: "BookTitles");

            migrationBuilder.RenameIndex(
                name: "IX_Books_CategoryId",
                table: "BookTitles",
                newName: "IX_BookTitles_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookTitles",
                table: "BookTitles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookCopies_BookTitles_BookId",
                table: "BookCopies",
                column: "BookId",
                principalTable: "BookTitles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookTitles_Categories_CategoryId",
                table: "BookTitles",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
