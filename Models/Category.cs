using System.ComponentModel.DataAnnotations;

namespace TunaCivataWeb.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        // BURAYI EKLEDİK: Görselin sunucudaki klasör yolunu burada saklayacağız.
        public string? ImagePath { get; set; } 
    }
}