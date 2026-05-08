using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers
{
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
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        // SHOW create form
        public IActionResult Create()
        {
            return View();
        }

        // SAVE new book
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                        "wwwroot", "images");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString()
                        + Path.GetExtension(
                            book.CoverImageFile.FileName);
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
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        // SAVE edited book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id) return NotFound();

            ModelState.Remove("Library");
            ModelState.Remove("LibraryId");
            ModelState.Remove("BorrowingTransactions");
            ModelState.Remove("Feedbacks");
            ModelState.Remove("CoverImageFile");

            if (ModelState.IsValid)
            {
                if (book.CoverImageFile != null)
                {
                    var uploadsFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot", "images");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString()
                        + Path.GetExtension(
                            book.CoverImageFile.FileName);
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

        // DELETE book
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // SEARCH books
        public async Task<IActionResult> Search(string query)
        {
            var results = await _context.Books
                .Where(b => b.Title.Contains(query)
                || b.Author.Contains(query))
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