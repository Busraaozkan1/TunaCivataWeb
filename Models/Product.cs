using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // IFormFile için gerekli

namespace TunaCivataWeb.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(100)] // Veritabanı optimizasyonu için eklenebilir
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Veritabanında null olmaması için varsayılan değer atandı
        public string ImageUrl { get; set; } = "/img/no-image.jpg";

        [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
        public string Category { get; set; } = string.Empty;

        // --- DOSYA YÜKLEME İÇİN EK ALAN ---
        // NotMapped: Bu alanın veritabanında bir sütun oluşturmamasını sağlar.
        // Sadece formdan gelen dosyayı geçici olarak tutmak için kullanılır.
        [NotMapped]
        public IFormFile? ImageFile { get; set; } 
    }
}