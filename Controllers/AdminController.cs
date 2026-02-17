using Microsoft.AspNetCore.Mvc;
using TunaCivataWeb.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TunaCivataWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // Admin yetki kontrolü
        private bool IsAdmin() => HttpContext.Session.GetString("IsAdmin") == "true";

        // --- GİRİŞ İŞLEMLERİ ---
        [HttpGet]
        public IActionResult Login()
        {
            if (IsAdmin()) return RedirectToAction("Index");
            
            Random rnd = new Random();
            int sayi1 = rnd.Next(1, 15);
            int sayi2 = rnd.Next(1, 15);
            HttpContext.Session.SetInt32("GuvenlikSonucu", sayi1 + sayi2);
            ViewBag.Soru = $"{sayi1} + {sayi2} = ?";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, int securityAnswer)
        {
            int? dogruSonuc = HttpContext.Session.GetInt32("GuvenlikSonucu");
            if (securityAnswer != dogruSonuc)
            {
                ViewBag.Error = "Güvenlik sorusu hatalı!";
                return Login();
            }

            // --- GEÇİCİ GÜNCELLEME: Şifre kontrolü devre dışı bırakıldı ---
            // Bu kısım senin içeri girip şifreni panelden düzeltmen için geçici olarak eklendi.
            HttpContext.Session.SetString("IsAdmin", "true");
            return RedirectToAction("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // --- ANA PANEL ---
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login");

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Messages = await _context.ContactMessages.OrderByDescending(x => x.CreatedDate).ToListAsync();
            var urunler = await _context.Products.OrderByDescending(x => x.Id).ToListAsync();
            return View(urunler);
        }

        // --- KATEGORİ İŞLEMLERİ ---
        [HttpPost]
        public async Task<IActionResult> AddCategory(string categoryName, IFormFile imageFile)
        {
            if (!IsAdmin()) return Unauthorized();

            if (!string.IsNullOrEmpty(categoryName))
            {
                var category = new Category { Name = categoryName };

                if (imageFile != null && imageFile.Length > 0)
                {
                    // Klasör Kontrolü (Railway Fix)
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "categories");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string path = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    category.ImagePath = "/img/categories/" + fileName;
                }

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(int id, string newName, IFormFile imageFile)
        {
            if (!IsAdmin()) return Unauthorized();

            var category = await _context.Categories.FindAsync(id);
            if (category != null && !string.IsNullOrEmpty(newName))
            {
                category.Name = newName;

                if (imageFile != null && imageFile.Length > 0)
                {
                    // Klasör Kontrolü (Railway Fix)
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "categories");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    if (!string.IsNullOrEmpty(category.ImagePath))
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", category.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string path = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    category.ImagePath = "/img/categories/" + fileName;
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var cat = await _context.Categories.FindAsync(id);
            if (cat != null)
            {
                if (!string.IsNullOrEmpty(cat.ImagePath))
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cat.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }

                _context.Categories.Remove(cat);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // --- ÜRÜN İŞLEMLERİ ---
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product)
        {
            if (!IsAdmin()) return Unauthorized();

            if (product.ImageFile != null)
            {
                product.ImageUrl = await SaveImage(product.ImageFile);
            }
            else
            {
                product.ImageUrl = "/img/no-image.jpg";
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product)
        {
            if (!IsAdmin()) return Unauthorized();

            var existingProduct = await _context.Products.FindAsync(product.Id);
            if (existingProduct == null) return NotFound();

            existingProduct.Name = product.Name;
            existingProduct.Category = product.Category;
            existingProduct.Description = product.Description;

            if (product.ImageFile != null)
            {
                DeletePhysicalFile(existingProduct.ImageUrl);
                existingProduct.ImageUrl = await SaveImage(product.ImageFile);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var urun = await _context.Products.FindAsync(id);
            if (urun != null)
            {
                DeletePhysicalFile(urun.ImageUrl);
                _context.Products.Remove(urun);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // --- YARDIMCI METODLAR ---
        private async Task<string> SaveImage(IFormFile file)
        {
            // Klasör Kontrolü (Railway Fix)
            string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            if (!Directory.Exists(wwwRootPath)) Directory.CreateDirectory(wwwRootPath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(wwwRootPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return "/img/" + fileName;
        }

        private void DeletePhysicalFile(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl) && imageUrl != "/img/no-image.jpg")
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }
        }

        public async Task<IActionResult> DeleteMessage(int id)
        {
            if (!IsAdmin()) return Unauthorized();
            var msg = await _context.ContactMessages.FindAsync(id);
            if (msg != null)
            {
                _context.ContactMessages.Remove(msg);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
