using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class CategoryPathVM
    {
        [Required(ErrorMessage = "Ниво 1 е задължително.")]
        [StringLength(100)]
        [Display(Name = "Ниво 1")]
        public string Level1 { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Ниво 2")]
        public string? Level2 { get; set; }

    }
}
