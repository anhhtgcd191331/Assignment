﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Assignment1.Data;
using Assignment1.Models;
using Assignment1.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Assignment1.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly UserContext _context;
        private readonly IEmailSender _emailSender;
        private readonly int _recordsPerPage = 5;
        private readonly int _recordsPerPages = 30;
        private readonly UserManager<Assignment1User> _userManager;

        public BooksController(UserContext context, UserManager<Assignment1User> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }
        public async Task<IActionResult> sendEmail()
        {
            await _emailSender.SendEmailAsync("hoangtienanhmk@gmail.com", "test send mail", "just test");
            return RedirectToAction("Index", "Carts");
        }

        // GET: Books
        //public async Task<IActionResult> Index()
        //{
        //    var userContext = _context.Book.Include(b => b.Store);
        //    return View(await userContext.ToListAsync());
        //}

        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Index(int id, string searchString)
        {
            Assignment1User thisUser = await _userManager.GetUserAsync(HttpContext.User);

            var thisStore = await _context.Store
                .FirstOrDefaultAsync(s => s.UserId == thisUser.Id);

            var userContext = _context.Book.Where(b => b.StoreId == thisStore.Id).Include(b => b.Store);
            var books1 = from b in userContext
                         select b;

            if (!String.IsNullOrEmpty(searchString))
            {
                books1 = books1.Where(s => s.Title!.Contains(searchString));
            }
            int numberOfRecords = await books1.CountAsync();     //Count SQL
            int numberOfPages = (int)Math.Ceiling((double)numberOfRecords / _recordsPerPage);
            ViewBag.numberOfPages = numberOfPages;
            ViewBag.currentPage = id;
            List<Book> books = await books1
                .Skip(id * _recordsPerPage)  //Offset SQL
                .Take(_recordsPerPage)       //Top SQL
                .ToListAsync();
            return View(books);
        }

        [AllowAnonymous]
        public async Task<IActionResult> List(int id, string searchString)
        {

            var books1 = from b in _context.Book
                         select b;

            if (!String.IsNullOrEmpty(searchString))
            {
                books1 = books1.Where(s => s.Title!.Contains(searchString) || s.Category.Contains(searchString));
            }
            int numberOfRecords = await books1.CountAsync();     //Count SQL
            int numberOfPages = (int)Math.Ceiling((double)numberOfRecords / _recordsPerPages);
            ViewBag.numberOfPages = numberOfPages;
            ViewBag.currentPage = id;
            List<Book> books = await books1
                .Skip(id * _recordsPerPages)  //Offset SQL
                .Take(_recordsPerPages)       //Top SQL
                .ToListAsync();
            return View(books);
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddToCart(string isbn)
        {
            string thisUserId = _userManager.GetUserId(HttpContext.User);

            var myCart = new CartItem() { UserID = thisUserId, BookIsbn = isbn, Quantity = 1 };
            var fromDb = _context.CartItem.FirstOrDefault(c => c.UserID == thisUserId && c.BookIsbn == isbn);
            //if not existing (or null), add it to cart. If already added to Cart before, ignore it.
            if (fromDb != null)
            {
                fromDb.Quantity++;
                _context.Update(fromDb);
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.Add(myCart);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("List");
        }
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Checkout()
        {
            string thisUserId = _userManager.GetUserId(HttpContext.User);
            List<CartItem> myDetailsInCart = await _context.CartItem
                .Where(c => c.UserID == thisUserId)
                .Include(c => c.Book)
                .ToListAsync();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {

                    //Step 1: create an order
                    Order myOrder = new Order();
                    myOrder.UserId = thisUserId;
                    myOrder.OrderTime = DateTime.Now;
                    myOrder.Total = myDetailsInCart.Select(c => c.Book.Price * c.Quantity)
                        .Aggregate((c1, c2) => Math.Round((c1 + c2), 1));

                    _context.Add(myOrder);
                    await _context.SaveChangesAsync();

                    //Step 2: insert all order details by var "myDetailsInCart"
                    foreach (var item in myDetailsInCart)
                    {
                        OrderDetail detail = new OrderDetail()
                        {
                            OrderId = myOrder.Id,
                            BookIsbn = item.BookIsbn,
                            //Quantity = int.Parse(Math.Round((myOrder.Total / item.Book.Price), 0).ToString())
                            Quantity = item.Quantity
                        };
                        _context.Add(detail);
                    }
                    await _context.SaveChangesAsync();


                    //Step 3: empty/delete the cart we just done for thisUser
                    _context.CartItem.RemoveRange(myDetailsInCart);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (DbUpdateException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Error occurred in Checkout" + ex);
                }
            }
            return RedirectToAction("sendEmail");
        }
        // GET: Books/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Store)
                .FirstOrDefaultAsync(m => m.Isbn == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["StoreId"] = new SelectList(_context.Store, "Id", "Id");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile image, [Bind("Isbn,Title,Pages,Author,Category,Price,Desc,ImgUrl")] Book book)
        {
            if (image != null)
            {
                //set key name
                string ImageName = book.Isbn + Path.GetExtension(image.FileName);

                string SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Image", ImageName);
                using (var stream = new FileStream(SavePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }
                book.ImgUrl = "img/" + ImageName;
                Assignment1User thisUser = await _userManager.GetUserAsync(HttpContext.User);
                Store thisStore = await _context.Store.FirstOrDefaultAsync(s => s.UserId == thisUser.Id);
                book.StoreId = thisStore.Id;
            }
            else
            {
                return View(book);
            }
            _context.Add(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            ViewData["StoreId"] = new SelectList(_context.Store, "Id", "Id", book.StoreId);

            return View(book);
        }
        [Authorize(Roles = "Seller")]
        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["StoreId"] = new SelectList(_context.Store, "Id", "Id", book.StoreId);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Edit(string id, [Bind("Isbn,Title,Pages,Author,Category,Price,Desc,ImgUrl")] Book book)
        {
            if (id != book.Isbn)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var bookToUpdate = await _context.Book.FirstOrDefaultAsync(s => s.Isbn == id);
                if (bookToUpdate == null)
                {
                    return NotFound();
                }
                bookToUpdate.Title = book.Title;
                bookToUpdate.Pages = book.Pages;
                bookToUpdate.Category = book.Category;
                bookToUpdate.Author = book.Author;
                bookToUpdate.Pages = book.Pages;
                bookToUpdate.Price = book.Price;
                bookToUpdate.Desc = book.Desc;
                try
                {
                    _context.Update(bookToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    ModelState.AddModelError("", "Unable to update the change. Error is: " + ex.Message);
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["StoreId"] = new SelectList(_context.Store, "Id", "Id", book.StoreId);
            return View(book);
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Store)
                .FirstOrDefaultAsync(m => m.Isbn == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [Authorize(Roles = "Seller")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return RedirectToAction(nameof(Index));
            }
            try
            {
                _context.Book.Remove(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Unable to delete book " + id + ". Error is: " + ex.Message);
                return NotFound();

            }
        }

        [AllowAnonymous]
        private bool BookExists(string id)
        {
            return _context.Book.Any(e => e.Isbn == id);
        }
    }
}
