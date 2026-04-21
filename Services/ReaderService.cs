using Microsoft.AspNetCore.Identity;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Services.Interfaces;

namespace ObreshkovLibrary.Services
{
    public class ReaderService : IReaderService
    {
        private readonly ObreshkovLibraryContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TemporaryPasswordService _temporaryPasswordService;
        private readonly CardNumberGenerator _cardNumberGenerator;

        public ReaderService(
            ObreshkovLibraryContext context,
            UserManager<IdentityUser> userManager,
            TemporaryPasswordService temporaryPasswordService,
            CardNumberGenerator cardNumberGenerator)
        {
            _context = context;
            _userManager = userManager;
            _temporaryPasswordService = temporaryPasswordService;
            _cardNumberGenerator = cardNumberGenerator;
        }

        public async Task<(bool Success, string[] Errors, string? GeneratedPassword)> CreateReaderAsync(Reader reader)
        {
            reader.CardNumber = await _cardNumberGenerator.GenerateUniqueAsync();
            reader.CreatedOn = DateTime.Now;
            reader.IsActive = true;

            _context.Add(reader);
            await _context.SaveChangesAsync();

            var generatedPassword = _temporaryPasswordService.Generate();
            reader.LastTemporaryPassword = generatedPassword;
            reader.PasswordChangedByReader = false;
            reader.LastPasswordChangeOn = null;

            _context.Update(reader);
            await _context.SaveChangesAsync();

            var readerUser = new IdentityUser
            {
                UserName = reader.CardNumber!.Trim().ToUpper(),
                Email = $"Reader_{reader.CardNumber.Trim().Replace("-", "").ToLower()}@obreshkov.local",
                EmailConfirmed = true
            };

            var createUserResult = await _userManager.CreateAsync(readerUser, generatedPassword);

            if (!createUserResult.Succeeded)
            {
                _context.Readers.Remove(reader);
                await _context.SaveChangesAsync();

                return (false, createUserResult.Errors.Select(e => e.Description).ToArray(), null);
            }

            var addRoleResult = await _userManager.AddToRoleAsync(readerUser, "Reader");

            if (!addRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(readerUser);
                _context.Readers.Remove(reader);
                await _context.SaveChangesAsync();

                return (false, addRoleResult.Errors.Select(e => e.Description).ToArray(), null);
            }

            return (true, Array.Empty<string>(), generatedPassword);
        }
    }
}