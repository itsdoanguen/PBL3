using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Notifications");

            migrationBuilder.AddColumn<int>(
                name: "ChapterID",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CommentID",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FromUserID",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSeen",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StoryID",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserModelUserID",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ChapterID",
                table: "Notifications",
                column: "ChapterID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CommentID",
                table: "Notifications",
                column: "CommentID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_FromUserID",
                table: "Notifications",
                column: "FromUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_StoryID",
                table: "Notifications",
                column: "StoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserModelUserID",
                table: "Notifications",
                column: "UserModelUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Chapters_ChapterID",
                table: "Notifications",
                column: "ChapterID",
                principalTable: "Chapters",
                principalColumn: "ChapterID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Comments_CommentID",
                table: "Notifications",
                column: "CommentID",
                principalTable: "Comments",
                principalColumn: "CommentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Stories_StoryID",
                table: "Notifications",
                column: "StoryID",
                principalTable: "Stories",
                principalColumn: "StoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_FromUserID",
                table: "Notifications",
                column: "FromUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserModelUserID",
                table: "Notifications",
                column: "UserModelUserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Chapters_ChapterID",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Comments_CommentID",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Stories_StoryID",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_FromUserID",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserModelUserID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ChapterID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CommentID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_FromUserID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_StoryID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserModelUserID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ChapterID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CommentID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "FromUserID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsSeen",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "StoryID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UserModelUserID",
                table: "Notifications");

            migrationBuilder.AddColumn<int>(
                name: "Title",
                table: "Notifications",
                type: "int",
                maxLength: 50,
                nullable: false,
                defaultValue: 0);
        }
    }
}
