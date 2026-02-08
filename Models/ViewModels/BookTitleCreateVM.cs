using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class BookTitleCreateVM
    {
        [Required] public string Title { get; set; } = "";
        [Required] public string Author { get; set; } = "";

        public int? Year { get; set; }
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }

        [MinLength(1, ErrorMessage = "Избери категория.")]
        public int[] CategoryIds { get; set; } = System.Array.Empty<int>();
    }
}
