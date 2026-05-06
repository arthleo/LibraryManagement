using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers
{
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
            var applicationDbContext = _context.BorrowingConfigs.Include(b => b.Library);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: BorrowingConfigs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrowingConfig = await _context.BorrowingConfigs
                .Include(b => b.Library)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (borrowingConfig == null)
            {
                return NotFound();
            }

            return View(borrowingConfig);
        }

        // GET: BorrowingConfigs/Create
        public IActionResult Create()
        {
            ViewData["LibraryId"] = new SelectList(_context.Libraries, "Id", "ContactDetails");
            return View();
        }

        // POST: BorrowingConfigs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
            ViewData["LibraryId"] = new SelectList(_context.Libraries, "Id", "ContactDetails", borrowingConfig.LibraryId);
            return View(borrowingConfig);
        }

        // GET: BorrowingConfigs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrowingConfig = await _context.BorrowingConfigs.FindAsync(id);
            if (borrowingConfig == null)
            {
                return NotFound();
            }
            ViewData["LibraryId"] = new SelectList(_context.Libraries, "Id", "ContactDetails", borrowingConfig.LibraryId);
            return View(borrowingConfig);
        }

        // POST: BorrowingConfigs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LoanDurationDays,RenewalLimit,OverduePenaltyPerDay,MaxBorrowableItems,LibraryId")] BorrowingConfig borrowingConfig)
        {
            if (id != borrowingConfig.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(borrowingConfig);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BorrowingConfigExists(borrowingConfig.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["LibraryId"] = new SelectList(_context.Libraries, "Id", "ContactDetails", borrowingConfig.LibraryId);
            return View(borrowingConfig);
        }

        // GET: BorrowingConfigs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrowingConfig = await _context.BorrowingConfigs
                .Include(b => b.Library)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (borrowingConfig == null)
            {
                return NotFound();
            }

            return View(borrowingConfig);
        }

        // POST: BorrowingConfigs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var borrowingConfig = await _context.BorrowingConfigs.FindAsync(id);
            if (borrowingConfig != null)
            {
                _context.BorrowingConfigs.Remove(borrowingConfig);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BorrowingConfigExists(int id)
        {
            return _context.BorrowingConfigs.Any(e => e.Id == id);
        }
    }
}
