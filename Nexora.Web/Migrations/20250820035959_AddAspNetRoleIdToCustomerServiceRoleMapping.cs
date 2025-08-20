using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexora.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAspNetRoleIdToCustomerServiceRoleMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AspNetRoleId",
                table: "CustomerServiceRoleMappings",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AspNetRoleId",
                table: "CustomerServiceRoleMappings");
        }
    }
}
