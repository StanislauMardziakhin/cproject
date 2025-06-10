using CourseProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CourseProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var template = new Template
            {
                Name = "Test Template",
                Description = "A test template for education",
                IsPublic = true,
                Theme = "Education"
            };
            _context.Templates.Add(template);
            await _context.SaveChangesAsync();
            
            var templates = await _context.Templates.ToListAsync();
            ViewData["Templates"] = templates;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}