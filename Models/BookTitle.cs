using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ObreshkovLibrary.Models
{
    public class BookTitle
    {
        public int Id { get; set; }

        [Required, StringLength(160)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string Author { get; set; } = string.Empty;

        public int? PublishYear { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? CoverImagePath { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();

        [NotMapped]
        public int AvailableCopies { get; set; }
    }
}
