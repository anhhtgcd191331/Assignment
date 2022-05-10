#nullable disable
using Assignment1.Areas.Identity.Data;
using Assignment1.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment1.Controllers
{
    public class StoresController : Controller
    {
        private readonly UserContext _context;
        private readonly UserManager<Assignment1User> _userManager;

        public StoresController(UserContext context, UserManager<Assignment1User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int id)
        {
            var storeQuery = _context.Store
                .Include(s => s.User);

            return View(await storeQuery.ToListAsync());
        }

        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Me()
        {
            var userId = _userManager.GetUserId(User);
            var store = await _context.Store.FirstOrDefaultAsync(s => s.UserId == userId);

            if (store == null)
                return RedirectToAction("Create");

            return View(store);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var store = await _context.Store
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (store == null)
            {
                return NotFound();
            }

            return View(store);
        }

        [HttpGet]
        [Authorize(Roles = "Seller")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Store store)
        {
            if (ModelState.IsValid)
            {
                store.UserId = _userManager.GetUserId(User);

                _context.Add(store);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Me));
            }

            return View(store);
        }

        [HttpGet]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User);
            var store = await _context.Store.FirstOrDefaultAsync(s => s.UserId == userId);

            if (store == null)
            {
                return NotFound();
            }

            return View(store);
        }

        // POST: Stores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Name,Address,Slogan")] Store model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var store = await _context.Store.FirstOrDefaultAsync(s => s.UserId == userId);

                if (store == null || store.Id != model.Id)
                    return BadRequest();

                try
                {
                    store.Name = model.Name;
                    store.Slogan = model.Slogan;
                    store.Address = model.Address;

                    _context.Update(store);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await StoreExistsAsync(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Me));
            }

            return View(model);
        }

        private async Task<bool> StoreExistsAsync(int id)
        {
            return await _context.Store.AnyAsync(e => e.Id == id);
        }
    }
}
