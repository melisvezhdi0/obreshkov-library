using System;
using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models
{
    public class LoanExtensionRequest
    {
        public int Id { get; set; }

        public int LoanId { get; set; }
        public Loan Loan { get; set; } = null!;

        [Range(3, 7, ErrorMessage = "Позволени са само 3, 5 или 7 дни.")]
        public int RequestedDays { get; set; }

        public LoanExtensionRequestStatus Status { get; set; } = LoanExtensionRequestStatus.Pending;

        public DateTime RequestedOn { get; set; } = DateTime.Now;

        public DateTime? ProcessedOn { get; set; }

        [StringLength(500)]
        public string? AdminResponseMessage { get; set; }
    }
}