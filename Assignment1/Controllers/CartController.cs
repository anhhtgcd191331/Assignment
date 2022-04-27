using Assignment1.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Assignment1.Controllers
{
    public class CartController : Controller
    {
        private readonly UserContext _context;

        public CartController(UserContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}