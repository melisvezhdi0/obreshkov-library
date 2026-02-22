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

        public string? CoverUrl { get; set; }

        public BookTags Tags { get; set; } = BookTags.None;

        public bool IsActive { get; set; } = true;

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();

        [NotMapped]
        public int AvailableCopies { get; set; }
    }
}