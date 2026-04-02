using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class SchoolNewsFormVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заглавието е задължително.")]
        [StringLength(160)]
        [Display(Name = "Заглавие")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Краткият текст е задължителен.")]
        [StringLength(700)]
        [Display(Name = "Кратък текст")]
        public string Summary { get; set; } = string.Empty;

        [Display(Name = "Снимка")]
        public IFormFile? ImageFile { get; set; }

        public string? CurrentImagePath { get; set; }

        [Required(ErrorMessage = "Линкът към новината е задължителен.")]
        [Url(ErrorMessage = "Въведи валиден линк.")]
        [Display(Name = "Линк към новината")]
        public string NewsUrl { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Дата")]
        public DateTime PublishedOn { get; set; } = DateTime.Today;

        [Display(Name = "Активна")]
        public bool IsActive { get; set; } = true;
    }
}