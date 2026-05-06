using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author is required")]
        [StringLength(150)]
        public string Author { get; set; }

        [Required(ErrorMessage = "Genre is required")]
        [StringLength(100)]
        public string Genre { get; set; }

        [Required(ErrorMessage = "ISBN is required")]
        [StringLength(13)]
        public string ISBN { get; set; }

        [StringLength(1000)]
        public string? Summary { get; set; }

        [Display(Name = "Cover Image")]
        public string? CoverImagePath { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        // FK
        public int LibraryId { get; set; }
        public Library? Library { get; set; }

        // Navigation
        public ICollection<BorrowingTransaction> BorrowingTransactions { get; set; } = new List<BorrowingTransaction>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

        [NotMapped]
        [Display(Name = "Upload Cover Image")]
        public IFormFile? CoverImageFile { get; set; }
    }
}