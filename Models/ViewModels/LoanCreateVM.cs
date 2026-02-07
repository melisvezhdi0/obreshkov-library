using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class LoanCreateVM
    {
        [Required]
        public int ClientId { get; set; }

        public string ClientName { get; set; } = "";
        public string CardNumber { get; set; } = "";

        [Required(ErrorMessage = "Заглавието е задължително.")]
        public string Title { get; set; } = "";

        public string? Author { get; set; }

        public string? Message { get; set; }     
        public string? ErrorMessage { get; set; } 
    }
}
