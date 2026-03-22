using System;
using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models
{
    public class SchoolNews
    {
        public int Id { get; set; }

        [Required]
        [StringLength(160)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(700)]
        public string Summary { get; set; } = string.Empty;

        [StringLength(300)]
        public string? ImagePath { get; set; }

        [Required]
        [StringLength(500)]
        [Url]
        public string NewsUrl { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime PublishedOn { get; set; } = DateTime.Today;

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}