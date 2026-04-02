using System;

namespace ObreshkovLibrary.Models
{
    public class BookAvailabilityRequest
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? NotifiedOn { get; set; }

        public DateTime? DeactivatedOn { get; set; }
    }
}