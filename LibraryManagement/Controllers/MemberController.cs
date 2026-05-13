using LibraryManagement.Data;
using LibraryManagement.Models;
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

        private async Task<BorrowingConfig> GetConfigAsync()
        {
            return await _db.BorrowingConfigs.FirstOrDefaultAsync()
                ?? new BorrowingConfig
                {
                    LoanDurationDays = 14,
                    RenewalLimit = 2,
                    OverduePenaltyPerDay = 0.50m,
                    MaxBorrowableItems = 5
                };
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

            var reservations = await _db.Reservations
                .Include(r => r.Book)
                .Where(r => r.UserId == userId && r.Status == "Reserved")
                .OrderByDescending(r => r.ReservedAt)
                .ToListAsync();

            ViewBag.ActiveLoans = activeLoans;
            ViewBag.History = history;
            ViewBag.AvailableBooks = availableBooks;
            ViewBag.MyReservations = reservations;

            ViewBag.UnpaidFines = await _db.BorrowingTransactions
                .Where(t => t.UserId == userId && t.FineAmount > 0 && !t.FinePaid)
                .SumAsync(t => t.FineAmount);

            ViewBag.OverdueCount = activeLoans.Count(t => t.DueDate < DateTime.Now);
            ViewBag.Config = config;

            return View();
        }

        public async Task<IActionResult> Browse(string? search, string? genre, string? availability)
        {
            var query = _db.Books
                .Include(b => b.Library)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    b.Title.Contains(search) ||
                    b.Author.Contains(search) ||
                    b.Genre.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                query = query.Where(b => b.Genre == genre);
            }

            if (!string.IsNullOrWhiteSpace(availability))
            {
                if (availability == "Available")
                {
                    query = query.Where(b => b.IsAvailable);
                }
                else if (availability == "Unavailable")
                {
                    query = query.Where(b => !b.IsAvailable);
                }
            }

            var books = await query
                .OrderBy(b => b.Title)
                .ToListAsync();

            var genres = await _db.Books
                .Where(b => b.Genre != null && b.Genre != "")
                .Select(b => b.Genre)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            ViewBag.Genres = genres;
            ViewBag.Search = search;
            ViewBag.Genre = genre;
            ViewBag.Availability = availability;

            return View(books);
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

            if (book == null)
            {
                TempData["Error"] = "Book not found.";
                return RedirectToAction("Browse");
            }

            if (!book.IsAvailable)
            {
                TempData["Error"] = "This book is not available. Please reserve it instead.";
                return RedirectToAction("Browse");
            }

            var transaction = new BorrowingTransaction
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

            var existingReservation = await _db.Reservations
                .FirstOrDefaultAsync(r =>
                    r.UserId == userId &&
                    r.BookId == bookId &&
                    r.Status == "Reserved");

            if (existingReservation != null)
            {
                existingReservation.Status = "Completed";
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = $"You borrowed \"{book.Title}\" for {config.LoanDurationDays} days.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve(int bookId)
        {
            var userId = _userManager.GetUserId(User);
            var book = await _db.Books.FindAsync(bookId);

            if (book == null)
            {
                TempData["Error"] = "Book not found.";
                return RedirectToAction("Browse");
            }

            if (book.IsAvailable)
            {
                TempData["Error"] = "This book is available. You can borrow it now.";
                return RedirectToAction("Browse");
            }

            var alreadyReserved = await _db.Reservations.AnyAsync(r =>
                r.UserId == userId &&
                r.BookId == bookId &&
                r.Status == "Reserved");

            if (alreadyReserved)
            {
                TempData["Error"] = "You already reserved this book.";
                return RedirectToAction("Browse");
            }

            var reservation = new Reservation
            {
                UserId = userId,
                BookId = bookId,
                ReservedAt = DateTime.Now,
                ExpiryDate = DateTime.Now.AddDays(3),
                Status = "Reserved"
            };

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"You reserved \"{book.Title}\".";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var userId = _userManager.GetUserId(User);

            var reservation = await _db.Reservations.FirstOrDefaultAsync(r =>
                r.Id == id &&
                r.UserId == userId &&
                r.Status == "Reserved");

            if (reservation == null)
            {
                TempData["Error"] = "Reservation not found.";
                return RedirectToAction("Index");
            }

            reservation.Status = "Cancelled";
            await _db.SaveChangesAsync();

            TempData["Success"] = "Reservation cancelled.";
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
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == userId &&
                    t.Status == "Borrowed");

            if (transaction == null)
            {
                TempData["Error"] = "Borrow record not found.";
                return RedirectToAction("Index");
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

            if (transaction.DueDate < DateTime.Now)
            {
                var config = await GetConfigAsync();
                var overdueDays = (DateTime.Now.Date - transaction.DueDate.Date).Days;

                if (overdueDays > 0)
                {
                    transaction.FineAmount = overdueDays * config.OverduePenaltyPerDay;
                    transaction.FinePaid = false;
                }
            }

            if (transaction.Book != null)
            {
                transaction.Book.IsAvailable = true;
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = $"\"{transaction.Book?.Title}\" returned successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveFeedback(int bookId, int rating, string? comment)
        {
            var userId = _userManager.GetUserId(User);

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Rating must be between 1 and 5.";
                return RedirectToAction("Index");
            }

            var alreadyLeft = await _db.Feedbacks
                .AnyAsync(f => f.UserId == userId && f.BookId == bookId);

            if (alreadyLeft)
            {
                TempData["Error"] = "You already left feedback for this book.";
                return RedirectToAction("Index");
            }

            _db.Feedbacks.Add(new Feedback
            {
                UserId = userId,
                BookId = bookId,
                Rating = rating,
                Comment = comment,
                SubmittedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "Thanks for your feedback!";
            return RedirectToAction("Index");
        }
    }
}