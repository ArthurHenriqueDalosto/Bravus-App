using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BravusApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class v003 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Duties_OperatorId_Date",
                table: "Duties");

            migrationBuilder.CreateIndex(
                name: "IX_Duties_OperatorId",
                table: "Duties",
                column: "OperatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Duties_OperatorId",
                table: "Duties");

            migrationBuilder.CreateIndex(
                name: "IX_Duties_OperatorId_Date",
                table: "Duties",
                columns: new[] { "OperatorId", "Date" },
                unique: true);
        }
    }
}
