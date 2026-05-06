using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Library
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Library name is required")]
        [StringLength(150)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(250)]
        public string Location { get; set; }

        [Required(ErrorMessage = "Operating hours are required")]
        [StringLength(200)]
        [Display(Name = "Operating Hours")]
        public string OperatingHours { get; set; }

        [Required(ErrorMessage = "Contact details are required")]
        [StringLength(200)]
        [Display(Name = "Contact Details")]
        public string ContactDetails { get; set; }

        // Navigation
        public ICollection<Book> Books { get; set; } = new List<Book>();
        public BorrowingConfig? BorrowingConfig { get; set; }
    }
}