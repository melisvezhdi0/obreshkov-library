using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }

        public ICollection<Category> Children { get; set; } = new List<Category>();

        public ICollection<BookTitle> BookTitles { get; set; } = new List<BookTitle>();
    }
}
