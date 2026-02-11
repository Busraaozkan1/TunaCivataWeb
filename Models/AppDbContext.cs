using Microsoft.EntityFrameworkCore;

namespace TunaCivataWeb.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // --- VERİTABANI TABLOLARI (DbSet) ---

        // Mevcut Ürünler Tablosu
        public DbSet<Product> Products { get; set; }

        // Dinamik Kategori Tablosu
        public DbSet<Category> Categories { get; set; }

        // Müşterilerden gelen "Bize Ulaşın" mesajları
        public DbSet<ContactMessage> ContactMessages { get; set; }

        // Admin giriş bilgilerini (Email ve Şifre) veritabanında tutmak için
        public DbSet<AdminUser> AdminUsers { get; set; }
    }
}