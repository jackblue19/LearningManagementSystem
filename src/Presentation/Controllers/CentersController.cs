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
    public class CentersController : Controller
    {
        private readonly CenterLmsContext _context;

        public CentersController(CenterLmsContext context)
        {
            _context = context;
        }

        // GET: Centers
        public async Task<IActionResult> Index()
        {
            var centerLmsContext = _context.Centers.Include(c => c.Manager);
            return View(await centerLmsContext.ToListAsync());
        }

        // GET: Centers/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var center = await _context.Centers
                .Include(c => c.Manager)
                .FirstOrDefaultAsync(m => m.CenterId == id);
            if (center == null)
            {
                return NotFound();
            }

            return View(center);
        }

        // GET: Centers/Create
        public IActionResult Create()
        {
            ViewData["ManagerId"] = new SelectList(_context.Users, "UserId", "Email");
            return View();
        }

        // POST: Centers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CenterId,CenterName,CenterAddress,Phone,CreatedAt,IsActive,CenterEmail,Logo,ManagerId")] Center center)
        {
            if (ModelState.IsValid)
            {
                center.CenterId = Guid.NewGuid();
                _context.Add(center);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ManagerId"] = new SelectList(_context.Users, "UserId", "Email", center.ManagerId);
            return View(center);
        }

        // GET: Centers/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var center = await _context.Centers.FindAsync(id);
            if (center == null)
            {
                return NotFound();
            }
            ViewData["ManagerId"] = new SelectList(_context.Users, "UserId", "Email", center.ManagerId);
            return View(center);
        }

        // POST: Centers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("CenterId,CenterName,CenterAddress,Phone,CreatedAt,IsActive,CenterEmail,Logo,ManagerId")] Center center)
        {
            if (id != center.CenterId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(center);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CenterExists(center.CenterId))
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
            ViewData["ManagerId"] = new SelectList(_context.Users, "UserId", "Email", center.ManagerId);
            return View(center);
        }

        // GET: Centers/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var center = await _context.Centers
                .Include(c => c.Manager)
                .FirstOrDefaultAsync(m => m.CenterId == id);
            if (center == null)
            {
                return NotFound();
            }

            return View(center);
        }

        // POST: Centers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var center = await _context.Centers.FindAsync(id);
            if (center != null)
            {
                _context.Centers.Remove(center);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CenterExists(Guid id)
        {
            return _context.Centers.Any(e => e.CenterId == id);
        }
    }
}
