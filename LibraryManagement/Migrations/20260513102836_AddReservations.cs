using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowingTransactions_Books_BookId1",
                table: "BorrowingTransactions");

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

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReservedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BookId",
                table: "Reservations",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId",
                table: "Reservations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations");

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
                name: "FK_BorrowingTransactions_Books_BookId1",
                table: "BorrowingTransactions",
                column: "BookId1",
                principalTable: "Books",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Books_BookId1",
                table: "Feedbacks",
                column: "BookId1",
                principalTable: "Books",
                principalColumn: "Id");
        }
    }
}
