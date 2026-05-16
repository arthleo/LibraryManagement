using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LIST all books
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.ToListAsync();
            return View(books);
        }

        // SHOW details
        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .Include(b => b.Library)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            return View(book);
        }

        // SHOW create form
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // SAVE new book
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Book book)
        {
            ModelState.Remove("Library");
            ModelState.Remove("LibraryId");
            ModelState.Remove("BorrowingTransactions");
            ModelState.Remove("Feedbacks");
            ModelState.Remove("CoverImageFile");

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (book.CoverImageFile != null)
                {
                    var uploadsFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "images");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString()
                                   + Path.GetExtension(book.CoverImageFile.FileName);

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await book.CoverImageFile.CopyToAsync(stream);
                    }

                    book.CoverImagePath = "/images/" + fileName;
                }

                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(book);
        }

        // SHOW edit form
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
                return NotFound();

            return View(book);
        }

        // SAVE edited book
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id)
                return NotFound();

            ModelState.Remove("Library");
            ModelState.Remove("LibraryId");
            ModelState.Remove("BorrowingTransactions");
            ModelState.Remove("Feedbacks");
            ModelState.Remove("CoverImageFile");

            if (ModelState.IsValid)
            {
                // Handle new image upload
                if (book.CoverImageFile != null)
                {
                    var uploadsFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "images");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString()
                                   + Path.GetExtension(book.CoverImageFile.FileName);

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await book.CoverImageFile.CopyToAsync(stream);
                    }

                    book.CoverImagePath = "/images/" + fileName;
                }

                _context.Books.Update(book);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(book);
        }

        // SHOW delete confirmation page
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books
                .Include(b => b.Library)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            return View(book);
        }

        // DELETE book after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
                return NotFound();

            // Delete related feedbacks
            var feedbacks = await _context.Feedbacks
                .Where(f => f.BookId == id)
                .ToListAsync();
            _context.Feedbacks.RemoveRange(feedbacks);

            // Delete related borrowing transactions
            var borrowings = await _context.BorrowingTransactions
                .Where(b => b.BookId == id)
                .ToListAsync();
            _context.BorrowingTransactions.RemoveRange(borrowings);

            // Delete the book
            _context.Books.Remove(book);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // SEARCH books
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction(nameof(Index));
            }

            var results = await _context.Books
                .Where(b =>
                    b.Title.Contains(query) ||
                    b.Author.Contains(query))
                .ToListAsync();

            return View("Index", results);
        }

        // FILTER by genre
        public async Task<IActionResult> FilterByGenre(string genre)
        {
            var results = await _context.Books
                .Where(b => b.Genre == genre)
                .ToListAsync();

            return View("Index", results);
        }
    }
}