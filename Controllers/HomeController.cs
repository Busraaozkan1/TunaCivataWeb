using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TunaCivataWeb.Models;

namespace TunaCivataWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Anasayfada ürünleri ve kategorileri göstermek için verileri çekiyoruz
        var products = await _context.Products.OrderByDescending(x => x.Id).Take(8).ToListAsync();
        ViewBag.Categories = await _context.Categories.ToListAsync();
        
        return View(products);
    }

    // --- YENİ: İLETİŞİM FORMU KAYIT METODU ---
    [HttpPost]
    public async Task<IActionResult> SendMessage(ContactMessage model)
    {
        if (ModelState.IsValid)
        {
            _context.ContactMessages.Add(model);
            await _context.SaveChangesAsync();
            
            // Mesaj başarıyla kaydedildi uyarısı için (Opsiyonel)
            TempData["SuccessMessage"] = "Mesajınız başarıyla iletildi!";
            return RedirectToAction("Index");
        }
        
        // Hata varsa tekrar anasayfaya dön
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> ProductsByCategory(string categoryName)
    {
        if (string.IsNullOrEmpty(categoryName))
        {
            return RedirectToAction("Index");
        }

        var urunler = await _context.Products
            .Where(p => p.Category == categoryName)
            .ToListAsync();

        ViewData["CategoryName"] = categoryName;
        // Kategorileri yan menüde listelemek istersen tekrar çekebiliriz
        ViewBag.Categories = await _context.Categories.ToListAsync();

        return View(urunler);
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