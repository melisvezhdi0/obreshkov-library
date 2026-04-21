using ObreshkovLibrary.Models.ViewModels;

namespace ObreshkovLibrary.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<HomeDashboardVM> BuildDashboardAsync(int latestLoansPage);
        Task<int?> FindReaderIdByCardNumberAsync(string cardNumber);
    }
}