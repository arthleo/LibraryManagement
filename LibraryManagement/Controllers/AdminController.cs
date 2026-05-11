using LibraryManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var totalBooks      = await _db.Books.CountAsync();
            var availableBooks  = await _db.Books.CountAsync(b => b.IsAvailable);
            var borrowedBooks   = await _db.Books.CountAsync(b => !b.IsAvailable);
            var totalMembers    = (await _userManager.GetUsersInRoleAsync("Member")).Count;
            var totalLibraries  = await _db.Libraries.CountAsync();

            var activeBorrows = await _db.BorrowingTransactions
                .Include(t => t.Book)
                .Include(t => t.User)
                .Where(t => t.Status == "Borrowed")
                .OrderByDescending(t => t.BorrowedAt)
                .Take(10)
                .ToListAsync();

            var overdueBorrows = await _db.BorrowingTransactions
                .Include(t => t.Book)
                .Include(t => t.User)
                .Where(t => t.Status == "Borrowed" && t.DueDate < DateTime.Now)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            var recentFeedback = await _db.Feedbacks
                .Include(f => f.Book)
                .Include(f => f.User)
                .OrderByDescending(f => f.SubmittedAt)
                .Take(5)
                .ToListAsync();

            var unpaidFines = await _db.BorrowingTransactions
                .Where(t => t.FineAmount > 0 && !t.FinePaid)
                .SumAsync(t => t.FineAmount);

            ViewBag.TotalBooks     = totalBooks;
            ViewBag.AvailableBooks = availableBooks;
            ViewBag.BorrowedBooks  = borrowedBooks;
            ViewBag.TotalMembers   = totalMembers;
            ViewBag.TotalLibraries = totalLibraries;
            ViewBag.OverdueCount   = overdueBorrows.Count;
            ViewBag.UnpaidFines    = unpaidFines;
            ViewBag.ActiveBorrows  = activeBorrows;
            ViewBag.OverdueBorrows = overdueBorrows;
            ViewBag.RecentFeedback = recentFeedback;

            return View();
        }

        // Users 
        public async Task<IActionResult> Users()
        {
            var members = await _userManager.GetUsersInRoleAsync("Member");
            var admins  = await _userManager.GetUsersInRoleAsync("Admin");
            ViewBag.Members = members;
            ViewBag.Admins  = admins;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PromoteToAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.RemoveFromRoleAsync(user, "Member");
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> DemoteToMember(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                await _userManager.AddToRoleAsync(user, "Member");
            }
            return RedirectToAction("Users");
        }

        // Borrowing Transactions 
        public async Task<IActionResult> Transactions()
        {
            var transactions = await _db.BorrowingTransactions
                .Include(t => t.Book)
                .Include(t => t.User)
                .OrderByDescending(t => t.BorrowedAt)
                .ToListAsync();
            return View(transactions);
        }

        [HttpPost]
        public async Task<IActionResult> MarkReturned(int id)
        {
            var t = await _db.BorrowingTransactions.FindAsync(id);
            if (t != null)
            {
                t.Status     = "Returned";
                t.ReturnedAt = DateTime.Now;
                var book = await _db.Books.FindAsync(t.BookId);
                if (book != null) book.IsAvailable = true;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Transactions");
        }
    }
}
