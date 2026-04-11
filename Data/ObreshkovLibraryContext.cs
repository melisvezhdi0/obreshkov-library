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
        public DbSet<Reader> Readers { get; set; } = null!;
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; } = null!;
        public DbSet<SchoolNews> SchoolNews { get; set; } = null!;
        public DbSet<ReaderFavoriteBook> ReaderFavoriteBooks { get; set; } = null!;
        public DbSet<LoanPersonalNote> LoanPersonalNotes { get; set; } = null!;
        public DbSet<ReaderNotification> ReaderNotifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reader>()
                .HasIndex(x => x.CardNumber)
                .IsUnique();

            modelBuilder.Entity<Loan>()
                .HasIndex(x => new { x.BookCopyId, x.ReturnDate });

            modelBuilder.Entity<Book>()
                .HasOne(x => x.Category)
                .WithMany(x => x.Books)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookCopy>()
                .HasOne(x => x.Book)
                .WithMany(x => x.Copies)
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PasswordResetRequest>()
                .HasOne(x => x.Reader)
                .WithMany()
                .HasForeignKey(x => x.ReaderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasOne(x => x.ParentCategory)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReaderFavoriteBook>()
                .HasOne(x => x.Reader)
                .WithMany(x => x.FavoriteBooks)
                .HasForeignKey(x => x.ReaderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReaderFavoriteBook>()
                .HasOne(x => x.Book)
                .WithMany(x => x.FavoritedByreaders)
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReaderFavoriteBook>()
                .HasIndex(x => new { x.ReaderId, x.BookId })
                .IsUnique();

            modelBuilder.Entity<LoanPersonalNote>()
                .HasOne(x => x.Loan)
                .WithMany(x => x.PersonalNotes)
                .HasForeignKey(x => x.LoanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReaderNotification>()
                .HasOne(x => x.Reader)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.ReaderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReaderNotification>()
                .HasOne(x => x.Book)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReaderNotification>()
                .HasOne(x => x.Loan)
                .WithMany()
                .HasForeignKey(x => x.LoanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reader>()
                .HasQueryFilter(x => x.IsActive);

            modelBuilder.Entity<Book>()
                .HasQueryFilter(x => x.IsActive);

            modelBuilder.Entity<Category>()
                .HasQueryFilter(x => x.IsActive);

            modelBuilder.Entity<BookCopy>()
                .HasQueryFilter(x => x.IsActive && x.Book.IsActive);

            modelBuilder.Entity<Loan>()
                .HasQueryFilter(x => x.Reader.IsActive && x.BookCopy.IsActive && x.BookCopy.Book.IsActive);

            modelBuilder.Entity<PasswordResetRequest>()
                .HasQueryFilter(x => x.Reader != null && x.Reader.IsActive);
        }
    }
}