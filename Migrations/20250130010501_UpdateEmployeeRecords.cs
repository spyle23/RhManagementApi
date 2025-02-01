using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmployeeRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRecord_Employees_EmployeeId",
                table: "EmployeeRecord");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeRecord",
                table: "EmployeeRecord");

            migrationBuilder.RenameTable(
                name: "EmployeeRecord",
                newName: "EmployeeRecords");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeRecord_EmployeeId",
                table: "EmployeeRecords",
                newName: "IX_EmployeeRecords_EmployeeId");

            migrationBuilder.AddColumn<decimal>(
                name: "GrossSalary",
                table: "EmployeeRecords",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeRecords",
                table: "EmployeeRecords",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRecords_Employees_EmployeeId",
                table: "EmployeeRecords",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRecords_Employees_EmployeeId",
                table: "EmployeeRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeRecords",
                table: "EmployeeRecords");

            migrationBuilder.DropColumn(
                name: "GrossSalary",
                table: "EmployeeRecords");

            migrationBuilder.RenameTable(
                name: "EmployeeRecords",
                newName: "EmployeeRecord");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeRecords_EmployeeId",
                table: "EmployeeRecord",
                newName: "IX_EmployeeRecord_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeRecord",
                table: "EmployeeRecord",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRecord_Employees_EmployeeId",
                table: "EmployeeRecord",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
