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
    public class BorrowingTransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BorrowingTransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BorrowingTransactions
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.BorrowingTransactions.Include(b => b.Book).Include(b => b.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: BorrowingTransactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrowingTransaction = await _context.BorrowingTransactions
                .Include(b => b.Book)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (borrowingTransaction == null)
            {
                return NotFound();
            }

            return View(borrowingTransaction);
        }

        // GET: BorrowingTransactions/Create
        public IActionResult Create()
        {
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Author");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: BorrowingTransactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,BookId,BorrowedAt,DueDate,ReturnedAt,Status,FineAmount,FinePaid,RenewalsUsed")] BorrowingTransaction borrowingTransaction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(borrowingTransaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Author", borrowingTransaction.BookId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", borrowingTransaction.UserId);
            return View(borrowingTransaction);
        }

        // GET: BorrowingTransactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrowingTransaction = await _context.BorrowingTransactions.FindAsync(id);
            if (borrowingTransaction == null)
            {
                return NotFound();
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Author", borrowingTransaction.BookId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", borrowingTransaction.UserId);
            return View(borrowingTransaction);
        }

        // POST: BorrowingTransactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,BookId,BorrowedAt,DueDate,ReturnedAt,Status,FineAmount,FinePaid,RenewalsUsed")] BorrowingTransaction borrowingTransaction)
        {
            if (id != borrowingTransaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(borrowingTransaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BorrowingTransactionExists(borrowingTransaction.Id))
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
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Author", borrowingTransaction.BookId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", borrowingTransaction.UserId);
            return View(borrowingTransaction);
        }

        // GET: BorrowingTransactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrowingTransaction = await _context.BorrowingTransactions
                .Include(b => b.Book)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (borrowingTransaction == null)
            {
                return NotFound();
            }

            return View(borrowingTransaction);
        }

        // POST: BorrowingTransactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var borrowingTransaction = await _context.BorrowingTransactions.FindAsync(id);
            if (borrowingTransaction != null)
            {
                _context.BorrowingTransactions.Remove(borrowingTransaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BorrowingTransactionExists(int id)
        {
            return _context.BorrowingTransactions.Any(e => e.Id == id);
        }
    }
}
