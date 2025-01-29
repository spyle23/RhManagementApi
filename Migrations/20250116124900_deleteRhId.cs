using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class deleteRhId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_RHs_RHId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_RHId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "RHId",
                table: "Employees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RHId",
                table: "Employees",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_RHId",
                table: "Employees",
                column: "RHId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_RHs_RHId",
                table: "Employees",
                column: "RHId",
                principalTable: "RHs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
