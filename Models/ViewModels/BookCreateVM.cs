using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using ObreshkovLibrary.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class BookCreateVM
    {
        [Required(ErrorMessage = "Моля, въведете заглавие.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Моля, въведете автор.")]
        public string Author { get; set; }

        public int? Year { get; set; }
        public string? Description { get; set; }
        public string? SearchKeywords { get; set; }

        public string? CoverPath { get; set; }
        public string? CurrentCoverPath { get; set; }
        public IFormFile? CoverFile { get; set; }

        public string? SchoolClass { get; set; }
        public List<string> SelectedSchoolClasses { get; set; } = new();

        [Required(ErrorMessage = "Моля, изберете категория.")]
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