using System.Collections.Generic;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class BookCreateVM
    {
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public int? Year { get; set; }
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }

        public int? Level1Id { get; set; }
        public int? Level2Id { get; set; }

        public List<Category> Level1Options { get; set; } = new();

        public string? TagsText { get; set; }

        public string? CardNumber { get; set; }
        public string? ErrorMessage { get; set; }

        public List<Loan> LatestLoans { get; set; } = new();
        public int DueTodayCount { get; set; }
        public List<Loan> DueTodayLoans { get; set; } = new();

        public int OverdueCount { get; set; }
        public List<Loan> OverdueLoans { get; set; } = new();

        public List<Book> LatestBookTitles { get; set; } = new();
    }
}