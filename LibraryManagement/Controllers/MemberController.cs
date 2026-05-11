using LibraryManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public MemberController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var activeLoans = await _db.BorrowingTransactions
                .Include(t => t.Book)
                .Where(t => t.UserId == userId && t.Status == "Borrowed")
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            var history = await _db.BorrowingTransactions
                .Include(t => t.Book)
                .Where(t => t.UserId == userId && t.Status == "Returned")
                .OrderByDescending(t => t.ReturnedAt)
                .Take(5)
                .ToListAsync();

            var availableBooks = await _db.Books
                .Include(b => b.Library)
                .Where(b => b.IsAvailable)
                .OrderBy(b => b.Title)
                .Take(12)
                .ToListAsync();

            var myFeedback = await _db.Feedbacks
                .Select(f => f.BookId)
                .Where(id => _db.Feedbacks.Any(f => f.UserId == userId && f.BookId == id))
                .ToListAsync();

            var unpaidFines = await _db.BorrowingTransactions
                .Where(t => t.UserId == userId && t.FineAmount > 0 && !t.FinePaid)
                .SumAsync(t => t.FineAmount);

            var overdueCount = activeLoans.Count(t => t.DueDate < DateTime.Now);

            ViewBag.ActiveLoans    = activeLoans;
            ViewBag.History        = history;
            ViewBag.AvailableBooks = availableBooks;
            ViewBag.MyFeedbackIds  = myFeedback;
            ViewBag.UnpaidFines    = unpaidFines;
            ViewBag.OverdueCount   = overdueCount;

            return View();
        }

        // ── Borrow a book ────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Borrow(int bookId)
        {
            var userId = _userManager.GetUserId(User);
            var book   = await _db.Books.FindAsync(bookId);

            if (book == null || !book.IsAvailable)
            {
                TempData["Error"] = "This book is no longer available.";
                return RedirectToAction("Index");
            }

            var transaction = new LibraryManagement.Models.BorrowingTransaction
            {
                UserId     = userId,
                BookId     = bookId,
                BorrowedAt = DateTime.Now,
                DueDate    = DateTime.Now.AddDays(14),
                Status     = "Borrowed"
            };

            book.IsAvailable = false;
            _db.BorrowingTransactions.Add(transaction);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"You borrowed \"{book.Title}\". Due back in 14 days.";
            return RedirectToAction("Index");
        }

        // ── Browse all available books ───────────────────────────────────────
        public async Task<IActionResult> Browse(string? search, string? genre)
        {
            var query = _db.Books.Include(b => b.Library).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(b =>
                    b.Title.Contains(search) || b.Author.Contains(search));

            if (!string.IsNullOrWhiteSpace(genre))
                query = query.Where(b => b.Genre == genre);

            var books  = await query.OrderBy(b => b.Title).ToListAsync();
            var genres = await _db.Books.Select(b => b.Genre).Distinct().ToListAsync();

            ViewBag.Genres = genres;
            ViewBag.Search = search;
            ViewBag.Genre  = genre;

            return View(books);
        }

        // ── Leave feedback ───────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> LeaveFeedback(int bookId, int rating, string? comment)
        {
            var userId = _userManager.GetUserId(User);

            var alreadyLeft = await _db.Feedbacks
                .AnyAsync(f => f.UserId == userId && f.BookId == bookId);

            if (!alreadyLeft)
            {
                _db.Feedbacks.Add(new LibraryManagement.Models.Feedback
                {
                    UserId      = userId,
                    BookId      = bookId,
                    Rating      = rating,
                    Comment     = comment,
                    SubmittedAt = DateTime.Now
                });
                await _db.SaveChangesAsync();
                TempData["Success"] = "Thanks for your feedback!";
            }

            return RedirectToAction("Index");
        }
    }
}
