using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    public class RegisteredRollNumbersController : Controller
    {
        private readonly MyContext _context;

        public RegisteredRollNumbersController(MyContext context)
        {
            _context = context;
        }
        public IActionResult Login(string ReturnUrl = "/")
        {
            LoginModel loginModel = new LoginModel();
            loginModel.ReturnUrl = ReturnUrl;
            return View(loginModel);
        }

        // GET: RegisteredRollNumbers
        public async Task<IActionResult> Index()
        {
              return _context.RegisteredRollNumber != null ? 
                          View(await _context.RegisteredRollNumber.ToListAsync()) :
                          Problem("Entity set 'MyContext.RegisteredRollNumber'  is null.");
        }
        [Authorize(Roles = "Admin")]

        // GET: RegisteredRollNumbers/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.RegisteredRollNumber == null)
            {
                return NotFound();
            }

            var registeredRollNumbers = await _context.RegisteredRollNumber
                .Include(r => r.Warranties)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (registeredRollNumbers == null)
            {
                return NotFound();
            }

            return View(registeredRollNumbers);
        }
        [Authorize(Roles = "Admin")]
        // GET: RegisteredRollNumbers/Create
        public IActionResult Create()
        {
            var viewModel = new RegisteredRollNumbers
            {
                Status = "Active",
                CategoryList = GetCategoryList()
            };
            return View(viewModel); 
        }

        // POST: RegisteredRollNumbers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Create(RegisteredRollNumbers registeredRollNumbers)
        {
            if (ModelState.IsValid)
            {
                registeredRollNumbers.Id = Guid.NewGuid();

                registeredRollNumbers.RegistrationDate = DateTime.SpecifyKind(registeredRollNumbers.RegistrationDate, DateTimeKind.Utc);
                registeredRollNumbers.Status = "Active";
                _context.Add(registeredRollNumbers);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            registeredRollNumbers.CategoryList = GetCategoryList();

            return View(registeredRollNumbers);
        }

        [Authorize(Roles = "Admin")]

        // GET: RegisteredRollNumbers/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.RegisteredRollNumber == null)
            {
                return NotFound();
            }

            var registeredRollNumbers = await _context.RegisteredRollNumber.FindAsync(id);
            if (registeredRollNumbers == null)
            {
                return NotFound();
            }
            return View(registeredRollNumbers);
        }

        // POST: RegisteredRollNumbers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(Guid id, [Bind("Id,RollNumber,Category,RegistrationDate,Status")] RegisteredRollNumbers registeredRollNumbers)
        {
            if (id != registeredRollNumbers.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    registeredRollNumbers.RegistrationDate = DateTime.SpecifyKind(registeredRollNumbers.RegistrationDate, DateTimeKind.Utc);
                    _context.Update(registeredRollNumbers);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegisteredRollNumbersExists(registeredRollNumbers.Id))
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
            return View(registeredRollNumbers);
        }
        [Authorize(Roles = "Admin")]

        // GET: RegisteredRollNumbers/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.RegisteredRollNumber == null)
            {
                return NotFound();
            }

            var registeredRollNumbers = await _context.RegisteredRollNumber
                .FirstOrDefaultAsync(m => m.Id == id);
            if (registeredRollNumbers == null)
            {
                return NotFound();
            }

            return View(registeredRollNumbers);
        }

        // POST: RegisteredRollNumbers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.RegisteredRollNumber == null)
            {
                return Problem("Entity set 'MyContext.RegisteredRollNumber'  is null.");
            }
            var registeredRollNumbers = await _context.RegisteredRollNumber.FindAsync(id);
            if (registeredRollNumbers != null)
            {
                _context.RegisteredRollNumber.Remove(registeredRollNumbers);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin")]

        private bool RegisteredRollNumbersExists(Guid id)
        {
          return (_context.RegisteredRollNumber?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private List<SelectListItem> GetCategoryList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "A", Text = "Prime" },
                new SelectListItem { Value = "B", Text = "Ultimate" },
                new SelectListItem { Value = "C", Text = "Ultimate Plus" }
            };
        }

    }
}
