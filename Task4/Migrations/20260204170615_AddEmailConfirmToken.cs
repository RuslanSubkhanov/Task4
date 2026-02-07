using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task4.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailConfirmToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmToken",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmToken",
                table: "Users");
        }
    }
}
