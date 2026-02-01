using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Data
{
    public class ObreshkovLibraryContext : DbContext
    {
        public ObreshkovLibraryContext(DbContextOptions<ObreshkovLibraryContext> options)
            : base(options)
        {
        }
        public DbSet<BookTitle> BookTitles { get; set; } = null!;
        public DbSet<BookCopy> BookCopies { get; set; } = null!;
        public DbSet<Loan> Loans { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
     
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.CardNumber)
                .IsUnique();

            modelBuilder.Entity<Loan>()
                .HasIndex(l => new { l.BookCopyId, l.ReturnDate });

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.CardNumber)
                .IsUnique();
        }
    }
}
