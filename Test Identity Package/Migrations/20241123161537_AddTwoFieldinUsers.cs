using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Test_Identity_Package.Migrations
{
    /// <inheritdoc />
    public partial class AddTwoFieldinUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Firstname",
                schema: "Secret",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "Secret",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Firstname",
                schema: "Secret",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "Secret",
                table: "Users");
        }
    }
}
