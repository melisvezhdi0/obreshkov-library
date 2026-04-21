using ObreshkovLibrary.Models.ViewModels;
using System.Security.Claims;

namespace ObreshkovLibrary.Services.Interfaces
{
    public interface IReaderNotificationService
    {
        Task<ReaderNotificationDropdownVM> BuildDropdownAsync(ClaimsPrincipal userPrincipal);

        Task ProcessLoanDueRemindersAsync();
    }
}