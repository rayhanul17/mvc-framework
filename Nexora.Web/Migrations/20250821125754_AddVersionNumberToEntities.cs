using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexora.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionNumberToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Table doesn't exist, skip dropping
            // migrationBuilder.DropTable(
            //     name: "CustomerServiceRoleMappings");

            // Skip dropping constraint that doesn't exist
            // migrationBuilder.DropUniqueConstraint(
            //     name: "AK_Roles_Name",
            //     table: "Roles");

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "TicketNotifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "TicketHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "TicketComments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "TicketAttachments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "SiteSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Skip altering Roles table
            // migrationBuilder.AlterColumn<string>(
            //     name: "Name",
            //     table: "Roles",
            //     type: "varchar(256)",
            //     maxLength: 256,
            //     nullable: true,
            //     oldClrType: typeof(string),
            //     oldType: "varchar(256)",
            //     oldMaxLength: 256)
            //     .Annotation("MySql:CharSet", "utf8mb4")
            //     .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "RoleMenus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "Menus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EntityVersionNumber",
                table: "Logs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "Logs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "LogArchives",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "FileDocuments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "BlogPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "BlogCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "TicketNotifications");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "TicketHistory");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "TicketComments");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "TicketAttachments");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "RoleMenus");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "EntityVersionNumber",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "LogArchives");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "FileDocuments");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "BlogCategories");

            // Skip updating Roles and recreating table
            // migrationBuilder.UpdateData(
            //     table: "Roles",
            //     keyColumn: "Name",
            //     keyValue: null,
            //     column: "Name",
            //     value: "");

            // migrationBuilder.AlterColumn<string>(
            //     name: "Name",
            //     table: "Roles",
            //     type: "varchar(256)",
            //     maxLength: 256,
            //     nullable: false,
            //     oldClrType: typeof(string),
            //     oldType: "varchar(256)",
            //     oldMaxLength: 256,
            //     oldNullable: true)
            //     .Annotation("MySql:CharSet", "utf8mb4")
            //     .OldAnnotation("MySql:CharSet", "utf8mb4");

            // migrationBuilder.AddUniqueConstraint(
            //     name: "AK_Roles_Name",
            //     table: "Roles",
            //     column: "Name");

            /* migrationBuilder.CreateTable(
                name: "CustomerServiceRoleMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AspNetRoleName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AspNetRoleId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerServiceRole = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerServiceRoleMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerServiceRoleMappings_Roles_AspNetRoleName",
                        column: x => x.AspNetRoleName,
                        principalTable: "Roles",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerServiceRoleMappings_AspNetRoleName",
                table: "CustomerServiceRoleMappings",
                column: "AspNetRoleName");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerServiceRoleMappings_CustomerServiceRole",
                table: "CustomerServiceRoleMappings",
                column: "CustomerServiceRole",
                unique: true); */
        }
    }
}
