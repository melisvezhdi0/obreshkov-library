using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required, StringLength(40)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(40)]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string Address { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string CardNumber { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}
