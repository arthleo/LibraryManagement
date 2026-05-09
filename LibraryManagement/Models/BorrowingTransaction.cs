using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class BorrowingTransaction
    {
        public int Id { get; set; }

        // FK - nullable to allow borrowing without user account
        public int? UserId { get; set; }
        public User? User { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        [Display(Name = "Borrowed At")]
        public DateTime BorrowedAt { get; set; } = DateTime.Now;

        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Returned At")]
        public DateTime? ReturnedAt { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Borrowed";

        [Display(Name = "Fine Amount ($)")]
        [Range(0, 10000)]
        public decimal FineAmount { get; set; } = 0;

        [Display(Name = "Fine Paid")]
        public bool FinePaid { get; set; } = false;

        [Display(Name = "Renewals Used")]
        public int RenewalsUsed { get; set; } = 0;
    }
}