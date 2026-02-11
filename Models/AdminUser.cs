using System.ComponentModel.DataAnnotations;

namespace TunaCivataWeb.Models
{
    public class AdminUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; } = "info@tunacivata.com";

        [Required]
        public string Password { get; set; } = "Tuna2026!";
    }
}