using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;


namespace LibraryManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BorrowingConfigsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BorrowingConfigsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BorrowingConfigs
        public async Task<IActionResult> Index()
        {
            var configs = await _context.BorrowingConfigs
                .Include(b => b.Library)
                .ToListAsync();
            return View(configs);
        }

        // GET: BorrowingConfigs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var config = await _context.BorrowingConfigs
                .Include(b => b.Library)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (config == null) return NotFound();
            return View(config);
        }

        // GET: BorrowingConfigs/Create
        public IActionResult Create()
        {
            ViewData["LibraryId"] = new SelectList(_context.Libraries, "Id", "Name");
            return View();
        }

        // POST: BorrowingConfigs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LoanDurationDays,RenewalLimit,OverduePenaltyPerDay,MaxBorrowableItems,LibraryId")] BorrowingConfig borrowingConfig)
        {
            if (ModelState.IsValid)
            {
                _context.Add(borrowingConfig);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LibraryId"] = new SelectList(_context.Libraries, "Id", "Name", borrowingConfig.LibraryId);
            return View(borrowingConfig);
        }

        // GET: BorrowingConfigs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var config = await _context.BorrowingConfigs.FindAsync(id);
            if (config == null) return NotFound();

            ViewData["LibraryId"] = new SelectList(_context.Libraries, "Id", "Name", config.LibraryId);
            return View(config);
        }

        // POST: BorrowingConfigs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LoanDurationDays,RenewalLimit,OverduePenaltyPerDay,MaxBorrowableItems,LibraryId")] BorrowingConfig borrowingConfig)
        {
            if (id != borrowingConfig.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(borrowingConfig);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BorrowingConfigExists(borrowingConfig.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["LibraryId"] = new SelectList(_context.Libraries, "Id", "Name", borrowingConfig.LibraryId);
            return View(borrowingConfig);
        }

        // GET: BorrowingConfigs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var config = await _context.BorrowingConfigs
                .Include(b => b.Library)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (config == null) return NotFound();
            return View(config);
        }

        // POST: BorrowingConfigs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var config = await _context.BorrowingConfigs.FindAsync(id);
            if (config != null) _context.BorrowingConfigs.Remove(config);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BorrowingConfigExists(int id) =>
            _context.BorrowingConfigs.Any(e => e.Id == id);
    }
}