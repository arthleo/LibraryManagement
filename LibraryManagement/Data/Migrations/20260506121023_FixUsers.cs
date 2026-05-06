using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowingTransactions_Users_UserId",
                table: "BorrowingTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Users_UserId",
                table: "Feedbacks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "LibraryUsers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LibraryUsers",
                table: "LibraryUsers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowingTransactions_LibraryUsers_UserId",
                table: "BorrowingTransactions",
                column: "UserId",
                principalTable: "LibraryUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_LibraryUsers_UserId",
                table: "Feedbacks",
                column: "UserId",
                principalTable: "LibraryUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowingTransactions_LibraryUsers_UserId",
                table: "BorrowingTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_LibraryUsers_UserId",
                table: "Feedbacks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LibraryUsers",
                table: "LibraryUsers");

            migrationBuilder.RenameTable(
                name: "LibraryUsers",
                newName: "Users");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowingTransactions_Users_UserId",
                table: "BorrowingTransactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Users_UserId",
                table: "Feedbacks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
