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
        //public async Task<IActionResult> Remove(int id)
        //{
        //    string thisUserId = _userManager.GetUserId(HttpContext.User);
        //    var movies = from m in _context.OrderDetail
        //                 select m;
        //    movies = movies.Where(s => s.BookIsbn!.Contains(id.ToString()));
        //    OrderDetail fromDb = _context.OrderDetail.FirstOrDefault(c => c.Order.UId == thisUserId && c.OrderId == id && c.BookIsbn == movies.ToString());
        //    _context.Remove(fromDb);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}
        //Assignment1User thisUser = await _userManager.GetUserAsync(HttpContext.User);
        //Store thisStore = await _context.Store.FirstOrDefaultAsync(s => s.UId == thisUser.Id);
        //Book thisBook = _context.Book.FirstOrDefault(b => b.StoreId == thisStore.Id);
        //var userContext = _context.OrderDetail.Where(o => o.BookIsbn == thisBook.Isbn);
        //    return View(await userContext.ToListAsync());
    }
}
