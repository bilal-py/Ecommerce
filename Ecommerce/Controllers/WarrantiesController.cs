using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

    namespace Ecommerce.Controllers
    {
        public class WarrantiesController : Controller
        {
            private readonly MyContext _context;
            private readonly IEmailService _emailService;
            private readonly IConfiguration _config;

            public WarrantiesController(MyContext context, IEmailService emailService, IConfiguration config)
            {
                _context = context;
                _emailService = emailService;
                _config = config;
            }

            [Authorize]
            // GET: Warranties
            public async Task<IActionResult> Index()
            {
                var myContext = _context.Warranties.Include(w => w.Customer).Include(w => w.Dealer).Include(w => w.RegisteredRollNumber);
            return View(await myContext.ToListAsync());
            }
            [Authorize]
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
                    .Include(w => w.RegisteredRollNumber)
                    .FirstOrDefaultAsync(m => m.WarrantyId == id);
                if (warranty == null)
                {
                    return NotFound();
                }

                return View(warranty);
            }
            
            [Authorize]
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
            [Authorize]
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

            [Authorize]
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
            [Authorize]
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
            
            [Authorize]
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
                    .Include(w => w.RegisteredRollNumber)
                    .FirstOrDefaultAsync(m => m.WarrantyId == id);
                if (warranty == null)
                {
                    return NotFound();
                }

                return View(warranty);
            }
            [Authorize]
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
            [Authorize]
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

            [Authorize]
            // GET: /Dashboard
            public IActionResult Dashboard()
            {
                ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName");
                ViewData["DealerId"] = new SelectList(_context.Dealers, "DealerId", "DealerName");


                return View();
            }
            [Authorize]
            // GET: /Dashboard/WarrantyRegistration
            public IActionResult WarrantyRegistration()
            {
                return PartialView("_WarrantyRegistration");
            }
        // GET: /Dashboard/ExpiredWarranty
        [Authorize]
        public IActionResult ExpiredWarranty()
            {
                return PartialView("_ExpiredWarranty");
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
                    .Include(w => w.RegisteredRollNumber)
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
                    .Include(w => w.RegisteredRollNumber)
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
        [Authorize]
        public async Task<IActionResult> SaveWarrantyFromDashboard(Warranty warranty, Customer customer, RegisteredRollNumbers registeredRollNumbers)
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
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                warranty.CustomerId = customer.CustomerId;
            }
            warranty.WarrantyStartDate = DateTime.SpecifyKind(warranty.WarrantyStartDate, DateTimeKind.Utc);
            warranty.WarrantyEndDate = DateTime.SpecifyKind(warranty.WarrantyEndDate, DateTimeKind.Utc);
            warranty.Customer = null;
            warranty.WarrantyId = Guid.NewGuid();
            warranty.Status = 0;

            //

            var registeredRollNumber = await _context.RegisteredRollNumber
                .FirstOrDefaultAsync(r => r.RollNumber == warranty.RollNumber);

            if (registeredRollNumber != null)
            {
                warranty.RegisteredRollNumberId = registeredRollNumber.Id;
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid roll number.";
                return RedirectToAction("Dashboard");
            }


            //Save the warranty
            _context.Warranties.Add(warranty);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Warranty submitted successfully!";

            /// email part
            var adminEmail = _config["Email:AdminEmail"];
            var dealer = await _context.Dealers.FindAsync(warranty.DealerId);



            // TEMP EMAILS (Replace with real ones)
           
            var dealerEmail = dealer?.Email ?? _config["Email:AdminEmail"];
            var customerEmail = customer?.Email ?? _config["Email:AdminEmail"];

            var sb = new StringBuilder();
            sb.AppendLine("<p>Hello</p>");

            string body = BuildWarrantyEmailTemplate(warranty, customer, dealer, registeredRollNumber);
            sb.AppendLine(body);

            string emailBody = sb.ToString();

            await _emailService.SendEmailAsync(
                to: customerEmail,
                subject: "Warranty Submitted",
                body: emailBody,
                cc: new List<string> { dealerEmail, adminEmail }
            );

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
                .Include(w => w.RegisteredRollNumber)
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
                .Include(w => w.RegisteredRollNumber)
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
            var warranty = await _context.Warranties
                .Include(w => w.Customer)
                .Include(w => w.Dealer)
                .Include(w => w.RegisteredRollNumber)
                .FirstOrDefaultAsync(w => w.WarrantyId == warrantyId);

            if (warranty == null) return NotFound();

            warranty.Status = status;
            await _context.SaveChangesAsync();

            // Prepare email details
            var customer = warranty.Customer;
            var dealer = warranty.Dealer;
            var registeredRollNumber = warranty.RegisteredRollNumber;

            var adminEmail = _config["Email:AdminEmail"];
            var dealerEmail = dealer?.Email ?? _config["Email:AdminEmail"];
            var customerEmail = customer?.Email ?? _config["Email:AdminEmail"];

            string decision = status == 1 ? "approved" : "rejected";

            string body;
            string subject = status == 1
                ? "Your MotoProtekt Warranty Has Been Approved!"
                : "Update on Your MotoProtekt Warranty Request";

            if (status == 1)
            {
                // build using the approved template above
                body = BuildApprovedEmailTemplate(warranty, customer, dealer, registeredRollNumber);
            }
            else
            {
                // build using the rejected template above
                body = BuildRejectedEmailTemplate(warranty, customer, dealer, registeredRollNumber);
            }



            await _emailService.SendEmailAsync(
                to: customerEmail,
                subject: subject,
                body: body,
                cc: new List<string> { dealerEmail, adminEmail }
            );

            return RedirectToAction("Dashboard");
        }


        [HttpGet]
        public IActionResult CheckRollNumberForDealer(string rollNumber)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            // Dealers can only use roll numbers that are:

            // Registered (by admin), and

            // Not already used by that dealer in previous submissions.
            

            if (user?.DealerId == null)
            {
                return Json(new { exists = false }); // or return error
            }
            bool rollNumberExists = _context.RegisteredRollNumber.Any(r =>
                r.RollNumber == rollNumber);
            bool exists = _context.Warranties.Any(w =>
                w.RollNumber == rollNumber && w.DealerId == user.DealerId);

            return Json(new { exists = exists || !rollNumberExists });
        }
        private string BuildWarrantyEmailTemplate(Warranty warranty, Customer customer, Dealer? dealer, RegisteredRollNumbers? registeredRollNumbers)
        {
            // Define the mappings
            var individualNames = new Dictionary<string, bool>
            {
                { "Bumpers - Front", warranty.BumpersFront },
                { "Hood - Leading Edge", warranty.HoodLead },
                { "Mirrors", warranty.Mirrors },
                { "Bumpers - Back", warranty.BumpersBack },
                { "Doors - Edge Guard", warranty.EdgeGuard },
                { "Windshield - Pillars", warranty.Windshield },
                { "Fenders - Leading Edge", warranty.FendersLead },
                { "Roof - Full", warranty.RoofFull },
                { "Hood - Full", warranty.HoodFull },
                { "Roof - Leading Edge", warranty.RoofLead },
                { "Headlamps", warranty.Headlamps },
                { "Trunk - Area", warranty.Trunk }
            };

            // Define what counts as a package
            var packageNames = new Dictionary<string, bool>
            {
                { "Vehicle Wrap - Full", warranty.VehicleWrapFull },
                { "Front - Full", warranty.FrontFull },
                { "Front - Leading Edge", warranty.FrontLeadingEdge }
            };




            string? categoryKey = registeredRollNumbers?.Category;
            var categoryDetails = new Dictionary<string, (string Name, string Description)>
            {
                { "A", ("Prime", "5yrs warranty on bubbling, chipping, and cracking & 1yr on yellowing") },
                { "B", ("Ultimate", "7yr warranty & 3yr warranty on yellowing") },
                { "C", ("Ultimate Plus", "10yrs warranty & 5yr on yellowing") }
            };

            string categoryName = "Unknown";
            string categoryDescription = "No warranty description available";

            if (!string.IsNullOrEmpty(categoryKey) && categoryDetails.TryGetValue(categoryKey, out var detail))
            {
                categoryName = detail.Name;
                categoryDescription = detail.Description;
            }






            var packageItems = packageNames.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
            var individualItems = individualNames.Where(kv => kv.Value).Select(kv => kv.Key).ToList();


            var sb = new StringBuilder();
            sb.AppendLine($"<p>Thank you for choosing MotoProtekt. Your Product Roll Number is <strong>{warranty.RollNumber}</strong>.</p>");
            sb.AppendLine($"<p>Series: <strong>{categoryName}</strong><br />Warranty Period: {categoryDescription}<br />For any queries please contact us at <a href='mailto:support@motoprotekt.de'>support@motoprotekt.de</a></p>");

            sb.AppendLine("<table border='1' cellpadding='6' cellspacing='0' style='border-collapse: collapse; font-family: Arial; font-size: 14px;'>");
            sb.AppendLine("<tr><th colspan='2' style='background-color: #f2f2f2;'>Vehicle Information</th></tr>");
            sb.AppendLine($"<tr><td><strong>Year</strong></td><td>{warranty.VehicleYear}</td></tr>");
            sb.AppendLine($"<tr><td><strong>Make</strong></td><td>{warranty.VehicleMake}</td></tr>");
            sb.AppendLine($"<tr><td><strong>Model</strong></td><td>{warranty.VehicleModel}</td></tr>");
            sb.AppendLine($"<tr><td><strong>Vin</strong></td><td>{warranty.VehicleVIN}</td></tr>");

            
            

            sb.AppendLine("<tr><td><strong>Packages</strong></td><td>");
            sb.AppendLine(string.Join(", ", packageItems));
            sb.AppendLine("</td></tr>");

            sb.AppendLine("<tr><td><strong>Individual</strong></td><td>");
            sb.AppendLine(string.Join(", ", individualItems));
            sb.AppendLine("</td></tr>");

            sb.AppendLine("<tr><td><strong>Additional Area</strong></td><td></td></tr>");
            sb.AppendLine("</table>");

            return sb.ToString();
        }

        private string BuildApprovedEmailTemplate(Warranty warranty, Customer customer, Dealer? dealer, RegisteredRollNumbers? registeredRollNumbers)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<p>Dear {customer.CustomerName},</p>");
            sb.AppendLine("<p>We're pleased to let you know that your warranty request has been <strong>approved</strong>!</p>");
            sb.AppendLine("<p>Below are your warranty details:</p>");
            sb.AppendLine(BuildWarrantyEmailTemplate(warranty, customer, dealer, registeredRollNumbers));
            sb.AppendLine("<p>If you have any questions, our support team is ready to assist—just reply to this email or call us at [support number].</p>");
            sb.AppendLine("<p>Thank you for choosing MotoProtekt!</p>");
            sb.AppendLine("<p>Warm regards,<br/>MotoProtekt Support Team</p>");
            return sb.ToString();
        }

        private string BuildRejectedEmailTemplate(Warranty warranty, Customer customer, Dealer? dealer, RegisteredRollNumbers? registeredRollNumbers)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<p>Dear {customer.CustomerName},</p>");
            sb.AppendLine("<p>Thank you for submitting your warranty request. Unfortunately, it has been <strong>rejected</strong>.</p>");
            sb.AppendLine($"<p><strong>Roll Number:</strong> {warranty.RollNumber}</p>");
            sb.AppendLine("<p>This decision could be due to one of the following:</p>");
            sb.AppendLine("<ul>" +
                          "<li>Invalid or unregistered roll number</li>" +
                          "<li>Warranty period has expired</li>" +
                          "<li>Required documentation/information missing</li>" +
                          "</ul>");
            sb.AppendLine("<p>If you'd like us to review again, please reply with additional details or contact our support team.</p>");
            sb.AppendLine("<p>We apologize for any inconvenience and are here to help.</p>");
            sb.AppendLine("<p>Best regards,<br/>MotoProtekt Support Team</p>");

            return sb.ToString();
        }

    }
}
