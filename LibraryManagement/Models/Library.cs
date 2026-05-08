using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Library
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public required string Name { get; set; }

        [Required]
        public required string Location { get; set; }

        public string? OperatingHours { get; set; }
        public string? ContactDetails { get; set; }
        public string? AdminId { get; set; }

        public ICollection<Book> Books { get; set; }
            = new List<Book>();
        public BorrowingConfig? BorrowingConfig { get; set; }
    }
}