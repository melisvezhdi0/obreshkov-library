using ObreshkovLibrary.Models;
using System.Collections.Generic;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class CatalogIndexVM
    {
        public List<Book> Books { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        public string Search { get; set; } = string.Empty;
        public string Sort { get; set; } = "date_desc";

        public int? SelectedCategoryId { get; set; }
        public string SelectedCategoryName { get; set; } = "Всички книги";
    }
}