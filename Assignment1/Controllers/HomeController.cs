using Assignment1.Areas.Identity.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using System.Diagnostics;

namespace Assignment1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<Assignment1User> _userManager;
        public HomeController(ILogger<HomeController> logger, UserManager<Assignment1User> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("from_address@example.com"));
            email.To.Add(MailboxAddress.Parse("to_address@example.com"));
            email.Subject = "Test Email Subject";
            email.Body = new TextPart(TextFormat.Plain) { Text = "Example Plain Text Message Body" };

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}