using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;

namespace ObreshkovLibrary.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<(List<Category> Categories, Dictionary<int, int> DirectBookCounts)> GetIndexDataAsync();
        Task<Category?> GetDetailsAsync(int id);
        Task<(bool Success, string? ErrorMessage)> DeactivateAsync(int id);
        Task CreatePathAsync(CategoryPathVM vm);
        Task<(int Id, string Name)> QuickCreateAsync(string name, int? parentCategoryId);
        Task<List<object>> GetRootsAsync();
        Task<List<object>> GetChildrenAsync(int parentId);
    }
}