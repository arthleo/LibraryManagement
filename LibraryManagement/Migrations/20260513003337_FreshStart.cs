using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.Migrations
{
    /// <inheritdoc />
    public partial class FreshStart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowingTransactions_AspNetUsers_UserId",
                table: "BorrowingTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_BorrowingTransactions_Books_BookId",
                table: "BorrowingTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_AspNetUsers_UserId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Books_BookId",
                table: "Feedbacks");

            migrationBuilder.AddColumn<int>(
                name: "BookId1",
                table: "Feedbacks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BookId1",
                table: "BorrowingTransactions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_BookId1",
                table: "Feedbacks",
                column: "BookId1");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingTransactions_BookId1",
                table: "BorrowingTransactions",
                column: "BookId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowingTransactions_AspNetUsers_UserId",
                table: "BorrowingTransactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowingTransactions_Books_BookId",
                table: "BorrowingTransactions",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowingTransactions_Books_BookId1",
                table: "BorrowingTransactions",
                column: "BookId1",
                principalTable: "Books",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_AspNetUsers_UserId",
                table: "Feedbacks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Books_BookId",
                table: "Feedbacks",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Books_BookId1",
                table: "Feedbacks",
                column: "BookId1",
                principalTable: "Books",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowingTransactions_AspNetUsers_UserId",
                table: "BorrowingTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_BorrowingTransactions_Books_BookId",
                table: "BorrowingTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_BorrowingTransactions_Books_BookId1",
                table: "BorrowingTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_AspNetUsers_UserId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Books_BookId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Books_BookId1",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_BookId1",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_BorrowingTransactions_BookId1",
                table: "BorrowingTransactions");

            migrationBuilder.DropColumn(
                name: "BookId1",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "BookId1",
                table: "BorrowingTransactions");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowingTransactions_AspNetUsers_UserId",
                table: "BorrowingTransactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowingTransactions_Books_BookId",
                table: "BorrowingTransactions",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_AspNetUsers_UserId",
                table: "Feedbacks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Books_BookId",
                table: "Feedbacks",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
