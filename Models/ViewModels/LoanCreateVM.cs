using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class LoanCreateVM
    {
        [Required(ErrorMessage = "Избери читател.")]
        [Display(Name = "Читател")]
        public int ReaderId { get; set; }

        [Display(Name = "Читател")]
        public string? ReaderName { get; set; }

        [Display(Name = "Номер на карта")]
        public string? CardNumber { get; set; }

        [Display(Name = "Заглавие")]
        public string? Title { get; set; }

        [Display(Name = "Автор")]
        public string? Author { get; set; }

        [Display(Name = "Книга")]
        public int? BookId { get; set; }

        [Display(Name = "Екземпляр")]
        public int? BookCopyId { get; set; }

        [Display(Name = "Съобщение за грешка")]
        public string? ErrorMessage { get; set; }

        [Display(Name = "Дата на заемане")]
        [DataType(DataType.Date)]
        public DateTime LoanDate { get; set; } = DateTime.Today;

        [Display(Name = "Срок за връщане")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

        public IEnumerable<SelectListItem> Readers { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> BookCopies { get; set; } = new List<SelectListItem>();
    }
}