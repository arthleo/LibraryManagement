using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace LibraryManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Home Page
        public async Task<IActionResult> Index()
        {
            // Books with cover images
            var books = await _context.Books
                .Where(b => !string.IsNullOrEmpty(b.CoverImagePath))
                .OrderByDescending(b => b.Id)
                .Take(6)
                .ToListAsync();

            // Active borrows — visible to everyone on homepage
            var activeBorrows = await _context.BorrowingTransactions
                .Include(t => t.Book)
                .Include(t => t.User)
                .Where(t => t.Status == "Borrowed")
                .OrderByDescending(t => t.BorrowedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.ActiveBorrows = activeBorrows;

            return View(books);
        }

        // Privacy Page
        public IActionResult Privacy()
        {
            return View();
        }

        // Error Page
        [ResponseCache(Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}
