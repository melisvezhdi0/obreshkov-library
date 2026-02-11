using System.Collections.Generic;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class BookTitleCreateVM
    {
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public int? Year { get; set; }
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }

        public int? Level1Id { get; set; }
        public int? Level2Id { get; set; }

        public List<int> SelectedCategoryIds { get; set; } = new();

        public List<Category> Level1Options { get; set; } = new();
    }
}
