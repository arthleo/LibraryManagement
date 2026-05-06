using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        // FK
        public int UserId { get; set; }
        public User? User { get; set; }

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