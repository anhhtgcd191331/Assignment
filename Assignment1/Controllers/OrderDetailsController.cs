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
    public class OrderDetailsController : Controller
    {
        private readonly UserContext _context;
        private readonly UserManager<Assignment1User> _userManager;
        public OrderDetailsController(UserContext context, UserManager<Assignment1User> userManager)
        {
            _context = context;
            _userManager =  userManager;  

        }
            // GET: OrderDetails
            [Authorize(Roles ="Seller")]
            public async Task<IActionResult> Index(int id)
        {
            var userContext = _context.OrderDetail.Where(o => o.OrderId == id).Include(o => o.Book).Include(o => o.Order).Include(o => o.Order.User).Include(o => o.Book.Store);
            return View(await userContext.ToListAsync());
        }
     
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ListOrders(int id)
        {
            string thisUserId = _userManager.GetUserId(HttpContext.User);
            var userContext = _context.OrderDetail.Where(o=>o.Order.UId==thisUserId && o.OrderId ==id).Include(o => o.Book).Include(o => o.Order).Include(o => o.Order.User).Include(o=>o.Book.Store);
            return View(await userContext.ToListAsync());
        }
        //public async Task<IActionResult> Remove(int id, string isbn)
        //{
        //    string thisUserId = _userManager.GetUserId(HttpContext.User);
        //    OrderDetail fromDb = _context.OrderDetail.FirstOrDefault(c => c.Order.UId == thisUserId && c.BookIsbn == isbn&& c.OrderId == id);
        //    _context.Remove(fromDb);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}
        
    }
}
