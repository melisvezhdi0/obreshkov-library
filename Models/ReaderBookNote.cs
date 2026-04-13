using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models
{
    public class ReaderBookNote
    {
        public int Id { get; set; }

        public int ReaderId { get; set; }
        public Reader Reader { get; set; } = null!;

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        [Required(ErrorMessage = "Бележката е задължителна.")]
        [StringLength(2000, ErrorMessage = "Бележката може да бъде най-много 2000 символа.")]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; }
    }
}