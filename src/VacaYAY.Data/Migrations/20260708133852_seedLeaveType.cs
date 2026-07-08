using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VacaYAY.Data.Migrations
{
    /// <inheritdoc />
    public partial class seedLeaveType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "LeaveTypes",
                columns: new[] { "Id", "Name", "Color", "IsPaid", "CountsAgainstBalance" },
                values: new object[,]
                {
                    { 1, "Annual", "#4CAF50", true,  true  },
                    { 2, "Sick",   "#F44336", true,  false },
                    { 3, "Paid",   "#2196F3", true,  false },
                    { 4, "Unpaid", "#9E9E9E", false, false },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4 });
        }
    }
}
