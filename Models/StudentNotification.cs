using System;
using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models
{
    public class StudentNotification
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        [Required]
        [StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        public StudentNotificationType Type { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public int? BookId { get; set; }
        public Book? Book { get; set; }

        public int? LoanId { get; set; }
        public Loan? Loan { get; set; }
    }
}