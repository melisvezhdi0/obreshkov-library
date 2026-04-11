using System;
using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models
{
    public class PasswordResetRequest
    {
        public int Id { get; set; }

        [Required]
        public int ReaderId { get; set; }

        [Required, StringLength(20)]
        public string CardNumber { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime RequestedOn { get; set; } = DateTime.Now;

        public bool IsCompleted { get; set; } = false;

        [StringLength(250)]
        public string? Notes { get; set; }

        [StringLength(50)]
        public string? GeneratedPassword { get; set; }

        public Reader? Reader { get; set; }
    }
}