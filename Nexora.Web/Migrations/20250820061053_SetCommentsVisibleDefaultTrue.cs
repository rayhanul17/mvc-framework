using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexora.Web.Migrations
{
    /// <inheritdoc />
    public partial class SetCommentsVisibleDefaultTrue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing records to have CommentsVisible = true
            migrationBuilder.Sql("UPDATE BlogPosts SET CommentsVisible = 1 WHERE CommentsVisible = 0");
            
            // Change default value to true
            migrationBuilder.AlterColumn<bool>(
                name: "CommentsVisible",
                table: "BlogPosts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Change default value back to false
            migrationBuilder.AlterColumn<bool>(
                name: "CommentsVisible",
                table: "BlogPosts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: true);
        }
    }
}
