using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3.Migrations
{
    /// <inheritdoc />
    public partial class InitHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoryModel",
                columns: table => new
                {
                    HistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    StoryID = table.Column<int>(type: "int", nullable: false),
                    ChapterID = table.Column<int>(type: "int", nullable: true),
                    LastReadAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryModel", x => x.HistoryID);
                    table.ForeignKey(
                        name: "FK_HistoryModel_Chapters_ChapterID",
                        column: x => x.ChapterID,
                        principalTable: "Chapters",
                        principalColumn: "ChapterID");
                    table.ForeignKey(
                        name: "FK_HistoryModel_Stories_StoryID",
                        column: x => x.StoryID,
                        principalTable: "Stories",
                        principalColumn: "StoryID");
                    table.ForeignKey(
                        name: "FK_HistoryModel_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryModel_ChapterID",
                table: "HistoryModel",
                column: "ChapterID");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryModel_StoryID",
                table: "HistoryModel",
                column: "StoryID");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryModel_UserID",
                table: "HistoryModel",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoryModel");
        }
    }
}
