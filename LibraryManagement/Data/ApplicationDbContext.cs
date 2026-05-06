using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;

namespace LibraryManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Library> Libraries { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowingConfig> BorrowingConfigs { get; set; }
        public DbSet<BorrowingTransaction> BorrowingTransactions { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BorrowingConfig>()
                .Property(b => b.OverduePenaltyPerDay)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<BorrowingTransaction>()
                .Property(b => b.FineAmount)
                .HasColumnType("decimal(10,2)");
        }
    }
}
