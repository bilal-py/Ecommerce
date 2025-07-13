using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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
            var myContext = _context.Warranties.Include(w => w.Customer).Include(w => w.Dealer);
            return View(await myContext.ToListAsync());
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
                .FirstOrDefaultAsync(m => m.WarrantyId == id);
            if (warranty == null)
            {
                return NotFound();
            }

            return View(warranty);
        }

        // GET: Warranties/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName");
            ViewData["DealerId"] = new SelectList(_context.Dealers, "DealerId", "DealerName");
            return View();
        }

        // POST: Warranties/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Warranty warranty, Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Check if customer exists
                var existingCustomer = _context.Customers.FirstOrDefault(c => c.Email == customer.Email);

                if (existingCustomer != null)
                {
                    warranty.CustomerId = existingCustomer.CustomerId;
                }
                else
                {
                    customer.CustomerId = Guid.NewGuid();
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                    warranty.CustomerId = customer.CustomerId;
                }

                warranty.WarrantyId = Guid.NewGuid();
                _context.Warranties.Add(warranty);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName", warranty.CustomerId);
            ViewData["DealerId"] = new SelectList(_context.Dealers, "DealerId", "DealerName", warranty.DealerId);
            return View(warranty);
        }

        // POST: Warranties/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("WarrantyId,RollNumber,Status,WarrantyStartDate,WarrantyEndDate,VehicleYear,VehicleMake,VehicleModel,VehicleVIN,CustomerId,DealerId,BumpersFront,HoodLead,Mirrors,BumpersBack,EdgeGuard,Windshield,FendersLead,RoofFull,HoodFull,RoofLead,Headlamps,Trunk")] Warranty warranty)
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName", warranty.CustomerId);
            ViewData["DealerId"] = new SelectList(_context.Dealers, "DealerId", "DealerName", warranty.DealerId);
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

        [HttpGet]
        public IActionResult GetCustomerByEmail(string email)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Email == email);
            if (customer == null) return Json(null);

            return Json(new
            {
                customerId = customer.CustomerId,
                customerName = customer.CustomerName,
                phoneNumber = customer.PhoneNumber,
                address = customer.Address,
                city = customer.City,
                state = customer.State,
                zip = customer.Zip
            });
        }


        // GET: /Dashboard
        public IActionResult Dashboard()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName");
            ViewData["DealerId"] = new SelectList(_context.Dealers, "DealerId", "DealerName");


            return View();
        }

        // GET: /Dashboard/WarrantyRegistration
        public IActionResult WarrantyRegistration()
        {
            return PartialView("_WarrantyRegistration");
        }

        // GET: /Dashboard/WarrantyPending
        [Authorize]
        public async Task<IActionResult> WarrantyPending(string dealerId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);
            var user = await _context.Users.FindAsync(userId);

            IQueryable<Warranty> query = _context.Warranties
                .Include(w => w.Customer)
                .Include(w => w.Dealer)
                .Where(w => w.Status == 0); // Pending

            if (role == "Dealer")
            {
                query = query.Where(w => w.DealerId == user.DealerId);
            }

            var warranties = await query.ToListAsync();

            return PartialView("_WarrantyPending", warranties); 
        }


        // GET: /Dashboard/WarrantyApproved
        [Authorize]
        public async Task<IActionResult> WarrantyApproved(String dealerId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);
            var user = await _context.Users.FindAsync(userId);

            IQueryable<Warranty> query = _context.Warranties
                .Include(w => w.Customer)
                .Include(w => w.Dealer)
                .Where(w => w.Status == 1); // Approved

            if (role == "Dealer")
            {
                query = query.Where(w => w.DealerId == user.DealerId);
            }

            var warranties = await query.ToListAsync();

            return PartialView("_WarrantyApproved", warranties);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveWarrantyFromDashboard(Warranty warranty, Customer customer)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.DealerId == null)
            {
                TempData["ErrorMessage"] = "Dealer not found or invalid role.";
                return RedirectToAction("Dashboard");
            }

            warranty.DealerId = user.DealerId.Value;

            Customer? existingCustomer = null;
            if (!string.IsNullOrEmpty(customer.Email))
            {
                existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Email.ToLower() == customer.Email.ToLower());
            }

            if (existingCustomer != null)
            {
                warranty.CustomerId = existingCustomer.CustomerId;
            }
            else
            {
                //customer.CustomerId = customer.CustomerId();
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                warranty.CustomerId = customer.CustomerId;
            }

            warranty.Customer = null; 
            warranty.WarrantyId = Guid.NewGuid();

            _context.Warranties.Add(warranty);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Warranty submitted successfully!";
            return RedirectToAction("Dashboard");
        }
        [Authorize]
        public async Task<IActionResult> WarrantyPendingPage(string dealerId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);
            var user = await _context.Users.FindAsync(userId);

            IQueryable<Warranty> query = _context.Warranties
                .Include(w => w.Customer)
                .Include(w => w.Dealer)
                .Where(w => w.Status == 0); // Pending

            if (role == "Dealer")
            {
                query = query.Where(w => w.DealerId == user.DealerId);
            }

            var warranties = await query.ToListAsync();

            return PartialView("_WarrantyPending", warranties); 
        }

        [Authorize]
        public async Task<IActionResult> WarrantyApprovedPage(string dealerId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role);
            var user = await _context.Users.FindAsync(userId);

            IQueryable<Warranty> query = _context.Warranties
                .Include(w => w.Customer)
                .Include(w => w.Dealer)
                .Where(w => w.Status == 1); // Approved

            if (role == "Dealer")
            {
                query = query.Where(w => w.DealerId == user.DealerId);
            }

            var warranties = await query.ToListAsync();

            return PartialView("_WarrantyApproved", warranties);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(Guid warrantyId, int status)
        {
            var warranty = await _context.Warranties.FindAsync(warrantyId);
            if (warranty == null) return NotFound();

            warranty.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public IActionResult CheckRollNumberForDealer(string rollNumber)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);


            if (user?.DealerId == null)
            {
                return Json(new { exists = false }); // or return error
            }

            bool exists = _context.Warranties.Any(w =>
                w.RollNumber == rollNumber && w.DealerId == user.DealerId);

            return Json(new { exists });
        }


    }
}
