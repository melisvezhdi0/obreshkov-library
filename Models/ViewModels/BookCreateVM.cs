using Microsoft.AspNetCore.Mvc.Rendering;
using ObreshkovLibrary.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class BookCreateVM
    {
        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Author { get; set; } = "";

        public int? Year { get; set; }
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }

        public int? Level1Id { get; set; }
        public int? Level2Id { get; set; }

        public List<Category> Level1Options { get; set; } = new();

        public List<int> SelectedTagValues { get; set; } = new();
        public List<SelectListItem> TagOptions { get; set; } = new();

        [Required]
        [Range(1, 50)]
        public int CopiesCount { get; set; } = 1;
    }
}