using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class BorrowingConfig
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 365)]
        [Display(Name = "Loan Duration (days)")]
        public int LoanDurationDays { get; set; } = 14;

        [Required]
        [Range(0, 10)]
        [Display(Name = "Renewal Limit")]
        public int RenewalLimit { get; set; } = 2;

        [Required]
        [Range(0, 100)]
        [Display(Name = "Overdue Penalty Per Day ($)")]
        public decimal OverduePenaltyPerDay { get; set; } = 0.50m;

        [Required]
        [Range(1, 50)]
        [Display(Name = "Max Borrowable Items")]
        public int MaxBorrowableItems { get; set; } = 5;

        // FK
        public int LibraryId { get; set; }
        public Library? Library { get; set; }
    }
}