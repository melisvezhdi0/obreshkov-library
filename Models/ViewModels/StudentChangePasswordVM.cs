using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class StudentChangePasswordVM
    {
        [Required(ErrorMessage = "Въведи текущата парола.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Въведи нова парола.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Паролата трябва да е поне {2} символа.", MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Потвърди новата парола.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Паролите не съвпадат.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}