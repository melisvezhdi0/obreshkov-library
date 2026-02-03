using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Име на категория")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Родителска категория")]
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }

        public ICollection<Category> Children { get; set; } = new List<Category>();

        public ICollection<BookTitle> BookTitles { get; set; } = new List<BookTitle>();
        public bool IsActive { get; set; } = true;
    }
}
