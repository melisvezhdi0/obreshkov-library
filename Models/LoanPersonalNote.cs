using System;
using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models
{
    public class LoanPersonalNote
    {
        public int Id { get; set; }

        public int LoanId { get; set; }
        public Loan Loan { get; set; } = null!;

        [Required(ErrorMessage = "Бележката е задължителна.")]
        [StringLength(1000, ErrorMessage = "Бележката може да бъде най-много 1000 символа.")]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; }
    }
}