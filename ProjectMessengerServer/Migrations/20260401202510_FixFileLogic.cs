using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectMessengerServer.Migrations
{
    /// <inheritdoc />
    public partial class FixFileLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileEntities_Users_UserId",
                table: "FileEntities");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "FileEntities",
                newName: "UploadedId");

            migrationBuilder.RenameColumn(
                name: "UploadedBy",
                table: "FileEntities",
                newName: "Purpose");

            migrationBuilder.RenameIndex(
                name: "IX_FileEntities_UserId",
                table: "FileEntities",
                newName: "IX_FileEntities_UploadedId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileEntities_Users_UploadedId",
                table: "FileEntities",
                column: "UploadedId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileEntities_Users_UploadedId",
                table: "FileEntities");

            migrationBuilder.RenameColumn(
                name: "UploadedId",
                table: "FileEntities",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Purpose",
                table: "FileEntities",
                newName: "UploadedBy");

            migrationBuilder.RenameIndex(
                name: "IX_FileEntities_UploadedId",
                table: "FileEntities",
                newName: "IX_FileEntities_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileEntities_Users_UserId",
                table: "FileEntities",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
