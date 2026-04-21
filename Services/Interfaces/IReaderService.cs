using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;

namespace ObreshkovLibrary.Services.Interfaces
{
    public interface IReaderService
    {
        Task<(bool Success, string? ErrorMessage)> CreateReaderAsync(Reader vm);
    }
}