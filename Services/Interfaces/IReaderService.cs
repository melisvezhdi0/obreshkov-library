using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Services.Interfaces
{
    public interface IReaderService
    {
        Task<(bool Success, string[] Errors, string? GeneratedPassword)> CreateReaderAsync(Reader reader);
    }
}