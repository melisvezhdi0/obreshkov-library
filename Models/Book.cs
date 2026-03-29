using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ObreshkovLibrary.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required, StringLength(160)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string Author { get; set; } = string.Empty;

        public int? Year { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public string? CoverPath { get; set; }

        public string? SchoolClass { get; set; }

        public BookTags Tags { get; set; } = BookTags.None;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();

        public ICollection<ClientFavoriteBook> FavoritedByClients { get; set; } = new List<ClientFavoriteBook>();

        public ICollection<BookAvailabilityRequest> AvailabilityRequests { get; set; } = new List<BookAvailabilityRequest>();

        public ICollection<StudentNotification> Notifications { get; set; } = new List<StudentNotification>();

        [NotMapped]
        public int AvailableCopies { get; set; }
    }
}