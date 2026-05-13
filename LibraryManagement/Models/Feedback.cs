using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace LibraryManagement.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        // Connected to ASP.NET Identity user
        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        [Display(Name = "Submitted At")]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}