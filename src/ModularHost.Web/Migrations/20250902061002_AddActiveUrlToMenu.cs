using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MRCMS.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveUrlToMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActiveUrl",
                table: "Menus",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveUrl",
                table: "Menus");
        }
    }
}
