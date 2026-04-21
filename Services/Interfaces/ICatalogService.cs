using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;
using System.Security.Claims;

namespace ObreshkovLibrary.Services.Interfaces
{
    public interface ICatalogService
    {
        Task<CatalogIndexVM> BuildCatalogIndexAsync(string? search, int? categoryId, string? sort);
        Task<Book?> BuildDetailsBookAsync(int id);
        Task FillDetailsViewBagsAsync(Book book, ClaimsPrincipal userPrincipal, dynamic viewBag);
        Task<List<int>> GetFavoriteBookIdsAsync(ClaimsPrincipal userPrincipal);
    }
}