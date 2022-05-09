#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Assignment1.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Identity;
using Assignment1.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;

namespace Assignment1.Controllers
{
    public class OrdersController : Controller
    {
        private readonly UserContext _context;
        private readonly UserManager<Assignment1User> _userManager;
        public OrdersController(UserContext context, UserManager<Assignment1User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            string thisUserId = _userManager.GetUserId(HttpContext.User);
            var userContext = _context.Order.Where(o => o.UId == thisUserId).Include(o => o.User);
            return View(await userContext.ToListAsync());
        }
        [Authorize(Roles ="Seller")]
        public async Task<IActionResult> Mana()
        {
            var userContext = _context.Order.Include(o => o.User);
            return View(await userContext.ToListAsync());
        }
        public async Task<IActionResult> Remove(int isbn)
        {
            string thisUserId = _userManager.GetUserId(HttpContext.User);
            Order fromDb = _context.Order.FirstOrDefault(c => c.UId == thisUserId && c.Id == isbn);

            _context.Remove(fromDb);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
