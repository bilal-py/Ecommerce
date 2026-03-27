using Ecommerce.Models;
using Ecommerce.Services;
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
    [Authorize(Roles = "Admin")]
    public class EmailLogsController : Controller
    {
        private readonly MyContext _context;
        private readonly IEmailService _emailService;


        public EmailLogsController(MyContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: EmailLogs
        public async Task<IActionResult> Index()
        {
              return _context.EmailLogs != null ? 
                          View(await _context.EmailLogs.ToListAsync()) :
                          Problem("Entity set 'MyContext.EmailLogs' is null.");
        }

        // GET: EmailLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.EmailLogs == null)
            {
                return NotFound();
            }

            var emailLog = await _context.EmailLogs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (emailLog == null)
            {
                return NotFound();
            }

            return View(emailLog);
        }

        // GET: EmailLogs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EmailLogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ToEmail,Cc,Subject,Body,EmailSent,CreatedAt,WarrantyId")] EmailLog emailLog)
        {
            if (ModelState.IsValid)
            {
                emailLog.CreatedAt = DateTime.SpecifyKind(emailLog.CreatedAt, DateTimeKind.Utc);
                _context.Add(emailLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(emailLog);
        }

        // GET: EmailLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.EmailLogs == null)
            {
                return NotFound();
            }

            var emailLog = await _context.EmailLogs.FindAsync(id);
            if (emailLog == null)
            {
                return NotFound();
            }
            return View(emailLog);
        }

        // POST: EmailLogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ToEmail,Cc,Subject,Body,EmailSent,CreatedAt,WarrantyId")] EmailLog emailLog)
        {
            if (id != emailLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    emailLog.CreatedAt = DateTime.SpecifyKind(emailLog.CreatedAt, DateTimeKind.Utc);
                    _context.Update(emailLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmailLogExists(emailLog.Id))
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
            return View(emailLog);
        }

        // GET: EmailLogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.EmailLogs == null)
            {
                return NotFound();
            }

            var emailLog = await _context.EmailLogs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (emailLog == null)
            {
                return NotFound();
            }

            return View(emailLog);
        }

        // POST: EmailLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.EmailLogs == null)
            {
                return Problem("Entity set 'MyContext.EmailLogs' is null.");
            }
            var emailLog = await _context.EmailLogs.FindAsync(id);
            if (emailLog != null)
            {
                _context.EmailLogs.Remove(emailLog);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmailLogExists(int id)
        {
          return (_context.EmailLogs?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // GET: EmailLogs/PendingEmailList
        public async Task<IActionResult> PendingEmailList()
        {
            if (_context.EmailLogs == null)
                return Problem("Entity set 'MyContext.EmailLogs' is null.");

            var pending = await _context.EmailLogs
                .Where(e => !e.EmailSent)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return View(pending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmails()
        {
            if (_context.EmailLogs == null)
                return Problem("Entity set 'MyContext.EmailLogs' is null.");

            // Step 1: Get all unsent emails
            var pendingEmails = await _context.EmailLogs
                .Where(e => !e.EmailSent)
                .ToListAsync();

            if (!pendingEmails.Any())
            {
                TempData["Message"] = "No pending emails to send.";
                return RedirectToAction(nameof(Index));
            }

            int successCount = 0;
            int failureCount = 0;

            foreach (var emailLog in pendingEmails)
            {
                try
                {
                    // Parse CC list if stored as comma-separated string
                    List<string>? ccList = null;
                    if (!string.IsNullOrWhiteSpace(emailLog.Cc))
                    {
                        ccList = emailLog.Cc
                            .Split(';', StringSplitOptions.RemoveEmptyEntries)
                            .Select(cc => cc.Trim())
                            .ToList();
                    }

                    // Step 2: Send the email
                    await _emailService.SendEmailAsync(
                        to: emailLog.ToEmail,
                        subject: emailLog.Subject,
                        body: emailLog.Body,
                        cc: ccList
                    );

                    // Step 3: Mark as sent
                    emailLog.EmailSent = true;
                    successCount++;
                }
                catch (Exception ex)
                {
                    failureCount++;
                    Console.WriteLine($"Failed to send email to {emailLog.ToEmail}: {ex.Message}");
                    // Optionally store error message in a new column if you have one (e.g., EmailError)
                }
            }

            // Step 4: Save all updates
            await _context.SaveChangesAsync();

            TempData["Message"] = $" {successCount} emails sent successfully. {failureCount} failed.";
            return RedirectToAction(nameof(Index));
        }

    }
}
