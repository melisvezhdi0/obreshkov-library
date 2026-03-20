using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObreshkovLibrary.Migrations
{
    public partial class RenameCover : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CoverUrl",
                table: "Books",
                newName: "CoverPath");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CoverPath",
                table: "Books",
                newName: "CoverUrl");
        }
    }
}