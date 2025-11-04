using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;

namespace Presentation.Controllers
{
    public class ClassRegistrationsController : Controller
    {
        private readonly CenterLmsContext _context;

        public ClassRegistrationsController(CenterLmsContext context)
        {
            _context = context;
        }

        // GET: ClassRegistrations
        public async Task<IActionResult> Index()
        {
            var centerLmsContext = _context.ClassRegistrations.Include(c => c.Class).Include(c => c.Student);
            return View(await centerLmsContext.ToListAsync());
        }

        // GET: ClassRegistrations/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classRegistration = await _context.ClassRegistrations
                .Include(c => c.Class)
                .Include(c => c.Student)
                .FirstOrDefaultAsync(m => m.RegistrationId == id);
            if (classRegistration == null)
            {
                return NotFound();
            }

            return View(classRegistration);
        }

        // GET: ClassRegistrations/Create
        public IActionResult Create()
        {
            ViewData["ClassId"] = new SelectList(_context.Classes, "ClassId", "ClassName");
            ViewData["StudentId"] = new SelectList(_context.Users, "UserId", "Email");
            return View();
        }

        // POST: ClassRegistrations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RegistrationId,ClassId,StudentId,RegisteredAt,RegistrationStatus")] ClassRegistration classRegistration)
        {
            if (ModelState.IsValid)
            {
                _context.Add(classRegistration);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClassId"] = new SelectList(_context.Classes, "ClassId", "ClassName", classRegistration.ClassId);
            ViewData["StudentId"] = new SelectList(_context.Users, "UserId", "Email", classRegistration.StudentId);
            return View(classRegistration);
        }

        // GET: ClassRegistrations/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classRegistration = await _context.ClassRegistrations.FindAsync(id);
            if (classRegistration == null)
            {
                return NotFound();
            }
            ViewData["ClassId"] = new SelectList(_context.Classes, "ClassId", "ClassName", classRegistration.ClassId);
            ViewData["StudentId"] = new SelectList(_context.Users, "UserId", "Email", classRegistration.StudentId);
            return View(classRegistration);
        }

        // POST: ClassRegistrations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("RegistrationId,ClassId,StudentId,RegisteredAt,RegistrationStatus")] ClassRegistration classRegistration)
        {
            if (id != classRegistration.RegistrationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(classRegistration);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassRegistrationExists(classRegistration.RegistrationId))
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
            ViewData["ClassId"] = new SelectList(_context.Classes, "ClassId", "ClassName", classRegistration.ClassId);
            ViewData["StudentId"] = new SelectList(_context.Users, "UserId", "Email", classRegistration.StudentId);
            return View(classRegistration);
        }

        // GET: ClassRegistrations/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classRegistration = await _context.ClassRegistrations
                .Include(c => c.Class)
                .Include(c => c.Student)
                .FirstOrDefaultAsync(m => m.RegistrationId == id);
            if (classRegistration == null)
            {
                return NotFound();
            }

            return View(classRegistration);
        }

        // POST: ClassRegistrations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var classRegistration = await _context.ClassRegistrations.FindAsync(id);
            if (classRegistration != null)
            {
                _context.ClassRegistrations.Remove(classRegistration);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClassRegistrationExists(long id)
        {
            return _context.ClassRegistrations.Any(e => e.RegistrationId == id);
        }
    }
}
