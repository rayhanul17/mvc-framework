using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MRCMS.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedMenuSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentContentType",
                table: "Comments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentFileName",
                table: "Comments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "Comments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "AttachmentSize",
                table: "Comments",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentContentType",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "AttachmentFileName",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "AttachmentSize",
                table: "Comments");
        }
    }
}
