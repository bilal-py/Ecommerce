using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Models;

namespace Ecommerce.Controllers
{
    public class WarrantiesController : Controller
    {
        private readonly MyContext _context;

        public WarrantiesController(MyContext context)
        {
            _context = context;
        }

        // GET: Warranties
        public async Task<IActionResult> Index()
        {
            var myContext = _context.Warranties.Include(w => w.Customer).Include(w => w.Dealer).Include(w => w.Product);
            return View(await myContext.ToListAsync());
        }


        // GET: Warranties/CheckRollNumber
        public IActionResult CheckRollNumber()
        {
            return View();
        }

        // POST: Warranties/CheckRollNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckRollNumber(Warranty warranty)
        {
            if (string.IsNullOrWhiteSpace(warranty.RollNumber))
            {
                TempData["Message"] = "Please enter a Roll Number.";
                return View();
            }

            var exists = _context.Warranties.Any(w => w.RollNumber == warranty.RollNumber);

            if (exists)
            {
                TempData["Message"] = "Roll Number already exists in the system!";
                return RedirectToAction("CheckRollNumber");
            }

            TempData["SuccessMessage"] = "Roll Number is valid! You can now create the warranty.";

            return RedirectToAction("Create", "Warranties", new { rollNumber = warranty.RollNumber });
        }

        // GET: Warranties/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Warranties == null)
            {
                return NotFound();
            }

            var warranty = await _context.Warranties
                .Include(w => w.Customer)
                .Include(w => w.Dealer)
                .Include(w => w.Product)
                .FirstOrDefaultAsync(m => m.WarrantyId == id);
            if (warranty == null)
            {
                return NotFound();
            }

            return View(warranty);
        }

        // GET: Warranties/Create
        public IActionResult Create(string? rollNumber)
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Email");
            ViewData["DealerId"] = new SelectList(_context.Dealers, "DealerId", "DealerName");
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId");

            var model = new Warranty();

            if (!string.IsNullOrEmpty(rollNumber))
            {
                model.RollNumber = rollNumber;
            }

            return View(model);
        }

        // POST: Warranties/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WarrantyId,RollNumber,Status,WarrantyStartDate,WarrantyEndDate,ProductId,CustomerId,DealerId")] Warranty warranty)
        {
            if (ModelState.IsValid)
            {
                warranty.WarrantyId = Guid.NewGuid();
                _context.Add(warranty);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Email", warranty.CustomerId);
            ViewData["DealerId"] = new SelectList(_context.Dealers, "DealerId", "DealerName", warranty.DealerId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", warranty.ProductId);
            return View(warranty);
        }

        // GET: Warranties/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Warranties == null)
            {
                return NotFound();
            }

            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Email", warranty.CustomerId);
            ViewData["DealerId"] = new SelectList(_context.Dealers, "DealerId", "DealerName", warranty.DealerId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", warranty.ProductId);
            return View(warranty);
        }

        // POST: Warranties/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("WarrantyId,RollNumber,Status,WarrantyStartDate,WarrantyEndDate,ProductId,CustomerId,DealerId")] Warranty warranty)
        {
            if (id != warranty.WarrantyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(warranty);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WarrantyExists(warranty.WarrantyId))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Email", warranty.CustomerId);
            ViewData["DealerId"] = new SelectList(_context.Dealers, "DealerId", "DealerName", warranty.DealerId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", warranty.ProductId);
            return View(warranty);
        }

        // GET: Warranties/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Warranties == null)
            {
                return NotFound();
            }

            var warranty = await _context.Warranties
                .Include(w => w.Customer)
                .Include(w => w.Dealer)
                .Include(w => w.Product)
                .FirstOrDefaultAsync(m => m.WarrantyId == id);
            if (warranty == null)
            {
                return NotFound();
            }

            return View(warranty);
        }

        // POST: Warranties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Warranties == null)
            {
                return Problem("Entity set 'MyContext.Warranties'  is null.");
            }
            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty != null)
            {
                _context.Warranties.Remove(warranty);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WarrantyExists(Guid id)
        {
          return (_context.Warranties?.Any(e => e.WarrantyId == id)).GetValueOrDefault();
        }
    }
}
