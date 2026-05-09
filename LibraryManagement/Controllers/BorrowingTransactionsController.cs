using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers
{
    public class BorrowingTransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BorrowingTransactionsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ADMIN - View all transactions
        public async Task<IActionResult> Index()
        {
            var transactions = await _context.BorrowingTransactions
                .Include(b => b.Book)
                .Include(b => b.User)
                .ToListAsync();
            return View(transactions);
        }

        // SHOW borrow form
        public async Task<IActionResult> Create()
        {
            var books = await _context.Books
                .Where(b => b.IsAvailable)
                .ToListAsync();
            ViewBag.Books = books;
            return View();
        }

        // MEMBER - Borrow a book
        [HttpPost]
        public async Task<IActionResult> BorrowBook(
            int bookId, int userId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null || !book.IsAvailable)
            {
                TempData["Error"] = "Book is not available.";
                return RedirectToAction("Create");
            }

            var borrow = new BorrowingTransaction
            {
                BookId = bookId,
                UserId = userId,
                BorrowedAt = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                Status = "Borrowed",
                FineAmount = 0,
                FinePaid = false,
                RenewalsUsed = 0
            };

            book.IsAvailable = false;

            // Disable foreign key check temporarily
            await _context.Database
                .ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF");
            _context.BorrowingTransactions.Add(borrow);
            await _context.SaveChangesAsync();
            await _context.Database
                .ExecuteSqlRawAsync("PRAGMA foreign_keys = ON");

            TempData["Success"] = "Book borrowed successfully!";
            return RedirectToAction("MyBorrows",
                new { userId = userId });
        }

        // MEMBER - View their borrows
        public async Task<IActionResult> MyBorrows(int userId)
        {
            var borrows = await _context.BorrowingTransactions
                .Include(b => b.Book)
                .Where(b => b.UserId == userId)
                .ToListAsync();
            return View(borrows);
        }

        // MEMBER - Return a book
        [HttpPost]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var borrow = await _context.BorrowingTransactions
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (borrow == null) return NotFound();

            borrow.ReturnedAt = DateTime.Now;
            borrow.Status = "Returned";
            borrow.Book.IsAvailable = true;

            // Calculate fine if overdue
            if (DateTime.Now > borrow.DueDate)
            {
                int overdueDays = (DateTime.Now - borrow.DueDate).Days;
                borrow.FineAmount = overdueDays * 1.00m;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Book returned successfully!";
            return RedirectToAction("MyBorrows",
                new { userId = borrow.UserId });
        }

        // MEMBER - Renew a book
        [HttpPost]
        public async Task<IActionResult> RenewBook(int id)
        {
            var borrow = await _context.BorrowingTransactions
                .FindAsync(id);

            if (borrow == null) return NotFound();

            if (borrow.RenewalsUsed >= 2)
            {
                TempData["Error"] =
                    "Maximum renewals reached. Cannot renew further.";
                return RedirectToAction("MyBorrows",
                    new { userId = borrow.UserId });
            }

            borrow.DueDate = borrow.DueDate.AddDays(14);
            borrow.RenewalsUsed++;
            borrow.Status = "Borrowed";

            await _context.SaveChangesAsync();
            TempData["Success"] = "Book renewed successfully!";
            return RedirectToAction("MyBorrows",
                new { userId = borrow.UserId });
        }

        // ADMIN - View overdue books
        public async Task<IActionResult> OverdueBooks()
        {
            var overdue = await _context.BorrowingTransactions
                .Include(b => b.Book)
                .Include(b => b.User)
                .Where(b => b.Status == "Borrowed"
                    && b.DueDate < DateTime.Now)
                .ToListAsync();
            return View(overdue);
        }

        // ADMIN - Borrowing report
        public async Task<IActionResult> Report()
        {
            var report = await _context.BorrowingTransactions
                .Include(b => b.Book)
                .Include(b => b.User)
                .OrderByDescending(b => b.BorrowedAt)
                .ToListAsync();
            return View(report);
        }

        // ADMIN - Mark fine as paid
        [HttpPost]
        public async Task<IActionResult> MarkFinePaid(int id)
        {
            var borrow = await _context.BorrowingTransactions
                .FindAsync(id);
            if (borrow == null) return NotFound();

            borrow.FinePaid = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Fine marked as paid!";
            return RedirectToAction("Index");
        }
    }
}