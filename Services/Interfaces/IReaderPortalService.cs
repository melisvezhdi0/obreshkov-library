using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;
using System.Security.Claims;

namespace ObreshkovLibrary.Services.Interfaces
{
    public interface IReaderPortalService
    {
        Task<Reader?> GetCurrentReaderAsync(ClaimsPrincipal userPrincipal);

        Task<ReaderDashboardVM?> BuildDashboardAsync(
            ClaimsPrincipal userPrincipal);
    }
}