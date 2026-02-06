using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required, StringLength(40)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? MiddleName { get; set; }

        [Required, StringLength(40)]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;



        [Required, StringLength(20)]
        public string CardNumber { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        [Required]
        [Range(1, 12)]
        public int? Grade { get; set; }

        [Required]
        [StringLength(2)]
        public string? Section { get; set; } 
    }
}
