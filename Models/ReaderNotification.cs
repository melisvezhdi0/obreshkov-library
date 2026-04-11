using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ObreshkovLibrary.Models.Enums;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Models
{
    public class ReaderNotification
    {
        public int Id { get; set; }

        [Required]
        public int ReaderId { get; set; }

        [ForeignKey(nameof(ReaderId))]
        public Reader Reader { get; set; } = null!;

        [Required]
        [MaxLength(120)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = null!;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; }

        public ReaderNotificationType Type { get; set; }

        public int? LoanId { get; set; }
        public Loan? Loan { get; set; }

        public int? BookId { get; set; }
        public Book? Book { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}