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
using Assignment1.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Assignment1.Controllers
{
    public class BooksController : Controller
    {
        private readonly UserContext _context;
        private readonly int _recordsPerPage = 5;
        private readonly UserManager<Assignment1User> _userManager;

        public BooksController(UserContext context, UserManager<Assignment1User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Books
        //public async Task<IActionResult> Index()
        //{
        //    var userContext = _context.Book.Include(b => b.Store);
        //    return View(await userContext.ToListAsync());
        //}
        public async Task<IActionResult> Index(int id)
        {
            Assignment1User thisUser = await _userManager.GetUserAsync(HttpContext.User);
            Store thisStore = await _context.Store.FirstOrDefaultAsync(s => s.UId == thisUser.Id);
        
        int numberOfRecords = await _context.Book.CountAsync();     //Count SQL
            int numberOfPages = (int)Math.Ceiling((double)numberOfRecords / _recordsPerPage);
            ViewBag.numberOfPages = numberOfPages;
            ViewBag.currentPage = id;
            List<Book> books = await _context.Book
                .Skip(id * _recordsPerPage)  //Offset SQL
                .Take(_recordsPerPage)       //Top SQL
                .ToListAsync();
            return View(books);
        }

        // GET: Books/Details/5
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
                book.Isbn = "img/" + ImageName;
                Assignment1User thisUser = await _userManager.GetUserAsync(HttpContext.User);
                Store thisStore = await _context.Store.FirstOrDefaultAsync(s => s.UId == thisUser.Id);
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
                bookToUpdate.Title = book.Title ; 
                bookToUpdate.Pages = book.Pages ;
                bookToUpdate.Category = book.Category ;
                bookToUpdate.Author = book.Author ;
                bookToUpdate.Pages = book.Pages;
                bookToUpdate.Price = book.Price ;
                bookToUpdate.Desc = book.Desc ;
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Isbn))
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
            ViewData["StoreId"] = new SelectList(_context.Store, "Id", "Id", book.StoreId);
            return View(book);
        }

        // GET: Books/Delete/5
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var book = await _context.Book.FindAsync(id);
            if(book == null)
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

        private bool BookExists(string id)
        {
            return _context.Book.Any(e => e.Isbn == id);
        }
    }
}
