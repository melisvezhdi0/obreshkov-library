using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Името е задължително.")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Фамилията е задължителна.")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Телефонният номер е задължителен.")]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(20)]
        public string? CardNumber { get; set; }

        public int? Grade { get; set; }

        [StringLength(10)]
        public string? Section { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        [StringLength(100)]
        public string? LastTemporaryPassword { get; set; }

        public bool PasswordChangedByStudent { get; set; } = false;

        public DateTime? LastPasswordChangeOn { get; set; }

        public ICollection<ClientFavoriteBook> FavoriteBooks { get; set; } = new List<ClientFavoriteBook>();

        public ICollection<StudentNotification> Notifications { get; set; } = new List<StudentNotification>();
    }
}