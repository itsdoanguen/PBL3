using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStyleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Styles_UserID",
                table: "Styles");

            migrationBuilder.CreateIndex(
                name: "IX_Styles_UserID",
                table: "Styles",
                column: "UserID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Styles_UserID",
                table: "Styles");

            migrationBuilder.CreateIndex(
                name: "IX_Styles_UserID",
                table: "Styles",
                column: "UserID");
        }
    }
}
