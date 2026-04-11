using System.ComponentModel.DataAnnotations.Schema;

namespace ObreshkovLibrary.Models
{
    public class ReaderFavoriteBook
    {
        public int Id { get; set; }

        public int readerId { get; set; }
        public Reader reader { get; set; } = null!;

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}