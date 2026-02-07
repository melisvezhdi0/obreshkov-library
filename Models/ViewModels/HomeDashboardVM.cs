using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class HomeDashboardVM
    {
        public string? CardNumber { get; set; }
        public Client? Client { get; set; }
        public List<Loan> ActiveLoans { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}
