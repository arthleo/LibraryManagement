using LibraryManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public MemberController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private async Task<LibraryManagement.Models.BorrowingConfig> GetConfigAsync()
        {
            var config = await _db.BorrowingConfigs.FirstOrDefaultAsync();

            if (config == null)
            {
                config = new LibraryManagement.Models.BorrowingConfig
                {
                    LoanDurationDays = 14,
                    RenewalLimit = 2,
                    OverduePenaltyPerDay = 0.50m,
                    MaxBorrowableItems = 5
                };
            }

            return config;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var config = await GetConfigAsync();

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
                .Where(f => f.UserId == userId)
                .Select(f => f.BookId)
                .ToListAsync();

            var unpaidFines = await _db.BorrowingTransactions
                .Where(t => t.UserId == userId && t.FineAmount > 0 && !t.FinePaid)
                .SumAsync(t => t.FineAmount);

            var overdueCount = activeLoans.Count(t => t.DueDate < DateTime.Now);

            ViewBag.ActiveLoans = activeLoans;
            ViewBag.History = history;
            ViewBag.AvailableBooks = availableBooks;
            ViewBag.MyFeedbackIds = myFeedback;
            ViewBag.UnpaidFines = unpaidFines;
            ViewBag.OverdueCount = overdueCount;
            ViewBag.Config = config;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(int bookId)
        {
            var userId = _userManager.GetUserId(User);
            var config = await GetConfigAsync();

            var currentBorrowedCount = await _db.BorrowingTransactions
                .CountAsync(t => t.UserId == userId && t.Status == "Borrowed");

            if (currentBorrowedCount >= config.MaxBorrowableItems)
            {
                TempData["Error"] = $"Borrow limit reached. You can only borrow {config.MaxBorrowableItems} books.";
                return RedirectToAction("Index");
            }

            var book = await _db.Books.FindAsync(bookId);

            if (book == null || !book.IsAvailable)
            {
                TempData["Error"] = "This book is no longer available.";
                return RedirectToAction("Index");
            }

            var transaction = new LibraryManagement.Models.BorrowingTransaction
            {
                UserId = userId,
                BookId = bookId,
                BorrowedAt = DateTime.Now,
                DueDate = DateTime.Now.AddDays(config.LoanDurationDays),
                Status = "Borrowed",
                FineAmount = 0,
                FinePaid = false,
                RenewalsUsed = 0
            };

            book.IsAvailable = false;

            _db.BorrowingTransactions.Add(transaction);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"You borrowed \"{book.Title}\" for {config.LoanDurationDays} days.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew(int id)
        {
            var userId = _userManager.GetUserId(User);
            var config = await GetConfigAsync();

            var transaction = await _db.BorrowingTransactions
                .Include(t => t.Book)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.RenewalsUsed >= config.RenewalLimit)
            {
                TempData["Error"] = $"Renewal limit reached. Maximum renewals allowed: {config.RenewalLimit}.";
                return RedirectToAction("Index");
            }

            transaction.DueDate = transaction.DueDate.AddDays(config.LoanDurationDays);
            transaction.RenewalsUsed += 1;

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Book renewed for another {config.LoanDurationDays} days.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Browse(string? search, string? genre)
        {
            var query = _db.Books.Include(b => b.Library).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    b.Title.Contains(search) ||
                    b.Author.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                query = query.Where(b => b.Genre == genre);
            }

            var books = await query.OrderBy(b => b.Title).ToListAsync();
            var genres = await _db.Books.Select(b => b.Genre).Distinct().ToListAsync();

            ViewBag.Genres = genres;
            ViewBag.Search = search;
            ViewBag.Genre = genre;

            return View(books);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveFeedback(int bookId, int rating, string? comment)
        {
            var userId = _userManager.GetUserId(User);

            var alreadyLeft = await _db.Feedbacks
                .AnyAsync(f => f.UserId == userId && f.BookId == bookId);

            if (!alreadyLeft)
            {
                _db.Feedbacks.Add(new LibraryManagement.Models.Feedback
                {
                    UserId = userId,
                    BookId = bookId,
                    Rating = rating,
                    Comment = comment,
                    SubmittedAt = DateTime.Now
                });

                await _db.SaveChangesAsync();

                TempData["Success"] = "Thanks for your feedback!";
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var userId = _userManager.GetUserId(User);

            var transaction = await _db.BorrowingTransactions
                .Include(t => t.Book)
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == userId &&
                    t.Status == "Borrowed");

            if (transaction == null)
            {
                TempData["Error"] = "Borrow record not found.";
                return RedirectToAction("Index");
            }

            transaction.Status = "Returned";
            transaction.ReturnedAt = DateTime.Now;

            if (transaction.Book != null)
            {
                transaction.Book.IsAvailable = true;
            }

            await _db.SaveChangesAsync();

            TempData["Success"] =
                $"\"{transaction.Book?.Title}\" returned successfully.";

            return RedirectToAction("Index");
        }
    }
}