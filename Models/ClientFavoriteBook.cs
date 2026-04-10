using System.ComponentModel.DataAnnotations.Schema;

namespace ObreshkovLibrary.Models
{
    public class ClientFavoriteBook
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}