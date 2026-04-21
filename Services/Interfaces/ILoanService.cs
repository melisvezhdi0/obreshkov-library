using System.Threading.Tasks;

namespace ObreshkovLibrary.Services.Interfaces
{
    public interface ILoanService
    {
        Task<bool> CreateLoanAsync(int readerId, int bookId);
        Task<bool> ReturnLoanAsync(int loanId);
    }
}