using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        public async Task<IActionResult> Index()
        {
            var transactions = await _context.BorrowingTransactions
                .Include(b => b.Book)
                .ToListAsync();
            return View(transactions);
        }

        public async Task<IActionResult> Create()
        {
            var books = await _context.Books
                .Where(b => b.IsAvailable)
                .ToListAsync();
            ViewBag.Books = books;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrowBook(int bookId)
        {
            try
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null || !book.IsAvailable)
                {
                    TempData["Error"] = "Book is not available.";
                    return RedirectToAction("Create");
                }

                var userId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

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
                _context.BorrowingTransactions.Add(borrow);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Book borrowed successfully!";
                return RedirectToAction("MyBorrows");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message
                    ?? "No inner exception";
                TempData["Error"] =
                    $"Error: {ex.Message} | Inner: {innerMessage}";
                return RedirectToAction("Create");
            }
        }

        public async Task<IActionResult> MyBorrows()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);
            var borrows = await _context.BorrowingTransactions
                .Include(b => b.Book)
                .Where(b => b.UserId == userId)
                .ToListAsync();
            return View(borrows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var borrow = await _context.BorrowingTransactions
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (borrow == null) return NotFound();

            borrow.ReturnedAt = DateTime.Now;
            borrow.Status = "Returned";

            if (borrow.Book != null)
                borrow.Book.IsAvailable = true;

            if (DateTime.Now > borrow.DueDate)
            {
                int overdueDays = (DateTime.Now - borrow.DueDate).Days;
                borrow.FineAmount = overdueDays * 1.00m;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Book returned successfully!";
            return RedirectToAction("MyBorrows");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewBook(int id)
        {
            var borrow = await _context.BorrowingTransactions
                .FindAsync(id);

            if (borrow == null) return NotFound();

            if (borrow.RenewalsUsed >= 2)
            {
                TempData["Error"] = "Maximum renewals reached.";
                return RedirectToAction("MyBorrows");
            }

            borrow.DueDate = borrow.DueDate.AddDays(14);
            borrow.RenewalsUsed++;
            borrow.Status = "Borrowed";

            await _context.SaveChangesAsync();
            TempData["Success"] = "Book renewed successfully!";
            return RedirectToAction("MyBorrows");
        }

        public async Task<IActionResult> OverdueBooks()
        {
            var overdue = await _context.BorrowingTransactions
                .Include(b => b.Book)
                .Where(b => b.Status == "Borrowed"
                    && b.DueDate < DateTime.Now)
                .ToListAsync();
            return View(overdue);
        }

        public async Task<IActionResult> Report()
        {
            var report = await _context.BorrowingTransactions
                .Include(b => b.Book)
                .OrderByDescending(b => b.BorrowedAt)
                .ToListAsync();
            return View(report);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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