using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Data
{
    public class ObreshkovLibraryContext : IdentityDbContext<Microsoft.AspNetCore.Identity.IdentityUser>
    {
        public ObreshkovLibraryContext(DbContextOptions<ObreshkovLibraryContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<BookCopy> BookCopies { get; set; } = null!;
        public DbSet<Loan> Loans { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; } = null!;
        public DbSet<SchoolNews> SchoolNews { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.CardNumber)
                .IsUnique();

            modelBuilder.Entity<Loan>()
                .HasIndex(l => new { l.BookCopyId, l.ReturnDate });

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookCopy>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.Copies)
                .HasForeignKey(bc => bc.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PasswordResetRequest>()
                .HasOne(r => r.Client)
                .WithMany()
                .HasForeignKey(r => r.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Client>()
                .HasQueryFilter(x => x.IsActive);

            modelBuilder.Entity<Book>()
                .HasQueryFilter(x => x.IsActive);

            modelBuilder.Entity<Category>()
                .HasQueryFilter(x => x.IsActive);

            modelBuilder.Entity<BookCopy>()
                .HasQueryFilter(x => x.IsActive && x.Book.IsActive);

            modelBuilder.Entity<Loan>()
                .HasQueryFilter(x => x.Client.IsActive && x.BookCopy.IsActive && x.BookCopy.Book.IsActive);

            modelBuilder.Entity<PasswordResetRequest>()
                .HasQueryFilter(x => x.Client != null && x.Client.IsActive);
        }
    }
}