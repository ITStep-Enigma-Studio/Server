using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectMessengerServer.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAvatarLogicForUsersAndChats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "UserProfiles");

            migrationBuilder.AddColumn<Guid>(
                name: "AvatarFileId",
                table: "UserProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AvatarFileId",
                table: "Chats",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_AvatarFileId",
                table: "UserProfiles",
                column: "AvatarFileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chats_AvatarFileId",
                table: "Chats",
                column: "AvatarFileId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_FileEntities_AvatarFileId",
                table: "Chats",
                column: "AvatarFileId",
                principalTable: "FileEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_FileEntities_AvatarFileId",
                table: "UserProfiles",
                column: "AvatarFileId",
                principalTable: "FileEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_FileEntities_AvatarFileId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_FileEntities_AvatarFileId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_AvatarFileId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Chats_AvatarFileId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "AvatarFileId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "AvatarFileId",
                table: "Chats");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "UserProfiles",
                type: "text",
                nullable: true);
        }
    }
}
