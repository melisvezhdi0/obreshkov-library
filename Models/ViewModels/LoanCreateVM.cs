using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class LoanCreateVM
    {
        [Required]
        public int ClientId { get; set; }

        public string ClientName { get; set; } = string.Empty;

        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Заглавието е задължително.")]
        public string Title { get; set; } = string.Empty;

        public string? Author { get; set; }

        public int? BookId { get; set; }

        public string? Message { get; set; }

        public string? ErrorMessage { get; set; }
    }
}