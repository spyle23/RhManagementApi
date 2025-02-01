using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class addreasonleave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Leaves",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Leaves");
        }
    }
}
