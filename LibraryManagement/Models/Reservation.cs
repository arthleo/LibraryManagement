using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        [Display(Name = "Reserved At")]
        public DateTime ReservedAt { get; set; } = DateTime.Now;

        [Display(Name = "Expiry Date")]
        public DateTime ExpiryDate { get; set; } = DateTime.Now.AddDays(3);

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Reserved";
    }
}