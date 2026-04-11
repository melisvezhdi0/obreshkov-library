using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Data.Seed
{
    public static class readerSeed
    {
        public static async Task SeedreadersAsync(
            ObreshkovLibraryContext context,
            UserManager<IdentityUser> userManager)
        {
            await NormalizeLegacySeedCardNumbersAsync(context);

            var oldSeedMarker = "2026001001";
            var newSeedMarker = "001001";

            bool alreadySeeded = await context.readers
                .IgnoreQueryFilters()
                .AnyAsync(c => c.CardNumber == oldSeedMarker || c.CardNumber == newSeedMarker);

            if (!alreadySeeded)
            {
                var firstNames = new[]
                {
                    "Иван","Георги","Николай","Димитър","Петър","Александър","Борис","Кристиян","Теодор","Мартин",
                    "Виктор","Симеон","Радослав","Стоян","Калоян","Йоан","Васил","Даниел","Преслав","Тодор",
                    "Мария","Елена","Виктория","Габриела","Никол","Теодора","Десислава","Йоана","Симона","Ивана",
                    "Дария","Михаела","Калина","Рая","Божидара","Надежда","Ралица","Полина","Анелия","Цветелина"
                };

                var middleNames = new[]
                {
                    "Иванов","Георгиев","Петров","Димитров","Николаев","Александров","Борисов","Стоянов","Тодоров","Василев",
                    "Радев","Христов","Йорданов","Стефанов","Кирилов","Колев","Трифонов","Павлов","Ангелов","Маринов"
                };

                var lastNamesMale = new[]
                {
                    "Иванов","Георгиев","Петров","Димитров","Николов","Александров","Борисов","Стоянов","Тодоров","Василев",
                    "Радев","Христов","Йорданов","Стефанов","Кирилов","Колев","Трифонов","Павлов","Ангелов","Маринов"
                };

                var lastNamesFemale = new[]
                {
                    "Иванова","Георгиева","Петрова","Димитрова","Николова","Александрова","Борисова","Стоянова","Тодорова","Василева",
                    "Радева","Христова","Йорданова","Стефанова","Кирилова","Колева","Трифонова","Павлова","Ангелова","Маринова"
                };

                var femaleNames = new HashSet<string>
                {
                    "Мария","Елена","Виктория","Габриела","Никол","Теодора","Десислава","Йоана","Симона","Ивана",
                    "Дария","Михаела","Калина","Рая","Божидара","Надежда","Ралица","Полина","Анелия","Цветелина"
                };

                var sections = new[] { "А", "Б", "В", "Г" };
                var readers = new List<Reader>();

                int cardCounter = 1001;
                int phoneCounter = 895082100;

                string NextCard() => (cardCounter++).ToString("D6");

                string NextPhone()
                {
                    var phone = "0" + phoneCounter.ToString("D9");
                    phoneCounter++;
                    return phone;
                }

                string GenerateTemporaryPassword(int grade, int index, bool isActive)
                {
                    int baseNumber = isActive ? 100000 : 200000;
                    return (baseNumber + grade * 10 + index).ToString();
                }

                Reader Createreader(int grade, int index, bool isActive, int createdDaysOffset)
                {
                    var first = firstNames[(grade * 10 + index) % firstNames.Length];
                    var middle = middleNames[(grade * 7 + index) % middleNames.Length];

                    bool female = femaleNames.Contains(first);

                    var last = female
                        ? lastNamesFemale[(grade * 5 + index) % lastNamesFemale.Length]
                        : lastNamesMale[(grade * 5 + index) % lastNamesMale.Length];

                    string? section = null;
                    if (grade >= 8)
                        section = sections[index % sections.Length];

                    return new Reader
                    {
                        FirstName = first,
                        MiddleName = middle,
                        LastName = last,
                        PhoneNumber = NextPhone(),
                        CardNumber = NextCard(),
                        Grade = grade,
                        Section = section,
                        IsActive = isActive,
                        CreatedOn = DateTime.Now.AddDays(-createdDaysOffset),
                        LastTemporaryPassword = GenerateTemporaryPassword(grade, index, isActive),
                        PasswordChangedByReader = false,
                        LastPasswordChangeOn = null
                    };
                }

                for (int grade = 5; grade <= 12; grade++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        readers.Add(Createreader(
                            grade: grade,
                            index: i,
                            isActive: true,
                            createdDaysOffset: grade * 10 + i));
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        readers.Add(Createreader(
                            grade: grade,
                            index: i + 20,
                            isActive: false,
                            createdDaysOffset: 300 + grade * 5 + i));
                    }
                }

                await context.readers.AddRangeAsync(readers);
                await context.SaveChangesAsync();
            }

            await EnsureSeededReaderUsersAsync(context, userManager);
        }

        private static async Task EnsureSeededReaderUsersAsync(
            ObreshkovLibraryContext context,
            UserManager<IdentityUser> userManager)
        {
            var readers = await context.readers
                .IgnoreQueryFilters()
                .Where(c => !string.IsNullOrWhiteSpace(c.CardNumber)
                            && !string.IsNullOrWhiteSpace(c.LastTemporaryPassword))
                .ToListAsync();

            foreach (var reader in readers)
            {
                var normalizedCardNumber = reader.CardNumber!.Trim().ToUpper();

                var existingUser = await userManager.FindByNameAsync(normalizedCardNumber);
                if (existingUser != null)
                {
                    if (!await userManager.IsInRoleAsync(existingUser, "Reader"))
                    {
                        await userManager.AddToRoleAsync(existingUser, "Reader");
                    }

                    continue;
                }

                var ReaderUser = new IdentityUser
                {
                    UserName = normalizedCardNumber,
                    Email = $"Reader_{normalizedCardNumber.ToLower()}@obreshkov.local",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(ReaderUser, reader.LastTemporaryPassword!);

                if (!result.Succeeded)
                {
                    var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Reader user seed failed for card {normalizedCardNumber}: {errors}");
                }

                var roleResult = await userManager.AddToRoleAsync(ReaderUser, "Reader");

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(" | ", roleResult.Errors.Select(e => e.Description));
                    throw new Exception($"Adding Reader role failed for card {normalizedCardNumber}: {errors}");
                }
            }
        }

        private static async Task NormalizeLegacySeedCardNumbersAsync(ObreshkovLibraryContext context)
        {
            var readers = await context.readers
                .IgnoreQueryFilters()
                .Where(c => !string.IsNullOrWhiteSpace(c.CardNumber))
                .ToListAsync();

            var legacyreaders = readers
                .Where(c => IsLegacySeedCardNumber(c.CardNumber!))
                .OrderBy(c => c.CardNumber)
                .ToList();

            if (!legacyreaders.Any())
                return;

            var legacyIds = legacyreaders
                .Select(c => c.Id)
                .ToHashSet();

            var usedSixDigitNumbers = readers
                .Where(c => !legacyIds.Contains(c.Id) && IsSixDigitCardNumber(c.CardNumber))
                .Select(c => c.CardNumber!)
                .ToHashSet();

            foreach (var reader in legacyreaders)
            {
                var proposedNumber = reader.CardNumber![^6..];

                if (usedSixDigitNumbers.Contains(proposedNumber))
                {
                    proposedNumber = await GenerateNextAvailableSixDigitCardNumberAsync(context, usedSixDigitNumbers);
                }

                reader.CardNumber = proposedNumber;
                usedSixDigitNumbers.Add(proposedNumber);
            }

            await context.SaveChangesAsync();
        }

        private static bool IsLegacySeedCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length != 10)
                return false;

            if (!cardNumber.StartsWith("2026"))
                return false;

            return cardNumber.All(char.IsDigit);
        }

        private static bool IsSixDigitCardNumber(string? cardNumber)
        {
            return !string.IsNullOrWhiteSpace(cardNumber)
                   && cardNumber.Length == 6
                   && cardNumber.All(char.IsDigit);
        }

        private static async Task<string> GenerateNextAvailableSixDigitCardNumberAsync(
            ObreshkovLibraryContext context,
            HashSet<string> reservedNumbers)
        {
            var maxExisting = await context.readers
                .IgnoreQueryFilters()
                .Where(c => c.CardNumber != null
                            && c.CardNumber.Length == 6
                            && c.CardNumber.All(char.IsDigit))
                .Select(c => c.CardNumber!)
                .ToListAsync();

            int nextNumber = 1001;

            if (maxExisting.Any())
            {
                nextNumber = maxExisting
                    .Select(x => int.Parse(x))
                    .Max() + 1;
            }

            while (reservedNumbers.Contains(nextNumber.ToString("D6")))
            {
                nextNumber++;
            }

            return nextNumber.ToString("D6");
        }
    }
}