using System;
using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models
{
    public class Loan
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public int BookCopyId { get; set; }
        public BookCopy BookCopy { get; set; } = null!;

        public DateTime LoanDate { get; set; } = DateTime.Now;

        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(30);

        public DateTime? ReturnDate { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }
    }
}