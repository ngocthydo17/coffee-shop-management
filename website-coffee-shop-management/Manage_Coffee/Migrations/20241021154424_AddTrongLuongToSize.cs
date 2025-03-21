using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manage_Coffee.Migrations
{
    /// <inheritdoc />
    public partial class AddTrongLuongToSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Trongluong",
                table: "Size",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Trongluong",
                table: "Size");
        }
    }
}
