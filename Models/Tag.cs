using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public ICollection<BookTitleTag> BookTitleTags { get; set; } = new List<BookTitleTag>();
    }
}
