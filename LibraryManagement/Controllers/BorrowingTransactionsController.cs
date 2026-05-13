using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BorrowingTransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BorrowingTransactionsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // All borrowing transactions
        public async Task<IActionResult> Index()
        {
            var transactions = await _context.BorrowingTransactions
                .Include(t => t.Book)
                .Include(t => t.User)
                .OrderByDescending(t => t.BorrowedAt)
                .ToListAsync();

            return View(transactions);
        }

        // Overdue books
        public async Task<IActionResult> OverdueBooks()
        {
            var overdue = await _context.BorrowingTransactions
                .Include(t => t.Book)
                .Include(t => t.User)
                .Where(t =>
                    t.Status == "Borrowed" &&
                    t.DueDate < DateTime.Now)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            return View(overdue);
        }

        // Borrowing report
        public async Task<IActionResult> Report()
        {
            var report = await _context.BorrowingTransactions
                .Include(t => t.Book)
                .Include(t => t.User)
                .OrderByDescending(t => t.BorrowedAt)
                .ToListAsync();

            return View(report);
        }

        // Mark fine as paid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFinePaid(int id)
        {
            var transaction =
                await _context.BorrowingTransactions.FindAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            transaction.FinePaid = true;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Fine marked as paid successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}