using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Book title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [Display(Name = "Book Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author name is required")]
        [StringLength(150, ErrorMessage = "Author name cannot exceed 150 characters")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Genre is required")]
        [StringLength(100, ErrorMessage = "Genre cannot exceed 100 characters")]
        public string Genre { get; set; }

        [Required(ErrorMessage = "ISBN is required")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN must be between 10 and 13 characters")]
        [Display(Name = "ISBN Number")]
        public string ISBN { get; set; }

        [StringLength(1000, ErrorMessage = "Summary cannot exceed 1000 characters")]
        public string? Summary { get; set; }

        [Display(Name = "Cover Image")]
        public string? CoverImagePath { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        // Foreign Key
        [Display(Name = "Library")]
        public int LibraryId { get; set; }

        // Navigation Property
        public Library? Library { get; set; }

        // Navigation Collections
        public ICollection<BorrowingTransaction> BorrowingTransactions { get; set; } = new List<BorrowingTransaction>();

        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

        // File Upload (Not stored in database)
        [NotMapped]
        [Display(Name = "Upload Cover Image")]
        public IFormFile? CoverImageFile { get; set; }
    }
}