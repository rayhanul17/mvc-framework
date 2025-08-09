using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamicRoleMenuSystem.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToAllEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Tickets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Tickets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TicketNotifications",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "TicketNotifications",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TicketHistory",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "TicketHistory",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TicketComments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "TicketComments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TicketAttachments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "TicketAttachments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "SiteSettings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "RoleMenus",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "RoleMenus",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Menus",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Menus",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "BlogPosts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "BlogPosts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "BlogCategories",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TicketNotifications");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "TicketNotifications");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TicketHistory");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "TicketHistory");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TicketComments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "TicketComments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TicketAttachments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "TicketAttachments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RoleMenus");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "RoleMenus");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "BlogCategories");
        }
    }
}
