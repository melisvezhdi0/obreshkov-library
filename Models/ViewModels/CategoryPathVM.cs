using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class CategoryPathVM
    {
        [Required(ErrorMessage = "Ниво 1 е задължително.")]
        [StringLength(100)]
        [Display(Name = "Ниво 1 (напр. Художествена литература)")]
        public string Level1 { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Ниво 2 (напр. Романи)")]
        public string? Level2 { get; set; }

        [StringLength(100)]
        [Display(Name = "Ниво 3 (напр. Класическа литература)")]
        public string? Level3 { get; set; }
    }
}
