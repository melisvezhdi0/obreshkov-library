using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using System.Security.Cryptography;

namespace ObreshkovLibrary.Services
{
    public class CardNumberGenerator
    {
        private readonly ObreshkovLibraryContext _db;

        public CardNumberGenerator(ObreshkovLibraryContext db)
        {
            _db = db;
        }

        public async Task<string> GenerateUniqueAsync()
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                var year = DateTime.Now.Year;
                var randomSixDigits = RandomNumberGenerator.GetInt32(100000, 1000000); 
                var cardNumber = $"OBR-{year}-{randomSixDigits}";

                bool exists = await _db.Clients.AnyAsync(c => c.CardNumber == cardNumber);
                if (!exists)
                    return cardNumber;
            }

            throw new InvalidOperationException("Грешка! Опитайте отново.");
        }
    }
}
