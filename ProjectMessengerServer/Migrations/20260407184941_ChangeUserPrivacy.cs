using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectMessengerServer.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserPrivacy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BackgroundFileId",
                table: "UserProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                @"ALTER TABLE ""UserPrivacies"" 
                    ALTER COLUMN ""ShowPhoneNumber"" TYPE integer 
                    USING CASE WHEN ""ShowPhoneNumber"" THEN 1 ELSE 0 END;");

            migrationBuilder.Sql(
                @"ALTER TABLE ""UserPrivacies"" 
                    ALTER COLUMN ""ShowLastSeen"" TYPE integer 
                    USING CASE WHEN ""ShowLastSeen"" THEN 1 ELSE 0 END;");

            migrationBuilder.Sql(
                @"ALTER TABLE ""UserPrivacies"" 
                    ALTER COLUMN ""ShowEmail"" TYPE integer 
                    USING CASE WHEN ""ShowEmail"" THEN 1 ELSE 0 END;");

            migrationBuilder.AddColumn<int>(
                name: "Birthday",
                table: "UserPrivacies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_BackgroundFileId",
                table: "UserProfiles",
                column: "BackgroundFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_FileEntities_BackgroundFileId",
                table: "UserProfiles",
                column: "BackgroundFileId",
                principalTable: "FileEntities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_FileEntities_BackgroundFileId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_BackgroundFileId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "BackgroundFileId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Birthday",
                table: "UserPrivacies");

            migrationBuilder.AlterColumn<bool>(
                name: "ShowPhoneNumber",
                table: "UserPrivacies",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "ShowLastSeen",
                table: "UserPrivacies",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "ShowEmail",
                table: "UserPrivacies",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
