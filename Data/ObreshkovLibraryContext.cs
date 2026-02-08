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
        public DbSet<BookTitleCategory> BookTitleCategories { get; set; } = null!;

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
            modelBuilder.Entity<BookTitleCategory>()
                .HasKey(x => new { x.BookTitleId, x.CategoryId });

            modelBuilder.Entity<BookTitleCategory>()
                .HasOne(x => x.BookTitle)
                .WithMany(bt => bt.BookTitleCategories)
                .HasForeignKey(x => x.BookTitleId);

            modelBuilder.Entity<BookTitleCategory>()
                .HasOne(x => x.Category)
                .WithMany(c => c.BookTitleCategories)
                .HasForeignKey(x => x.CategoryId);


            modelBuilder.Entity<Client>().HasQueryFilter(x => x.IsActive);
            modelBuilder.Entity<BookTitle>().HasQueryFilter(x => x.IsActive);
            modelBuilder.Entity<BookCopy>().HasQueryFilter(x => x.IsActive);
            modelBuilder.Entity<Category>().HasQueryFilter(x => x.IsActive);

        }
    }
}
