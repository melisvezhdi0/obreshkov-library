using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Data.Seed
{
    public static class ClientSeed
    {
        public static async Task SeedClientsAsync(ObreshkovLibraryContext context)
        {
            var seedMarker = "2026001001";

            bool alreadySeeded = await context.Clients
                .IgnoreQueryFilters()
                .AnyAsync(c => c.CardNumber == seedMarker);

            if (alreadySeeded)
                return;

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

            var clients = new List<Client>();

            int cardCounter = 1001;
            int phoneCounter = 895082100;

            string NextCard()
                => $"2026{cardCounter++.ToString("D6")}";

            string NextPhone()
            {
                var phone = "0" + phoneCounter.ToString("D9");
                phoneCounter++;
                return phone;
            }

            for (int grade = 5; grade <= 12; grade++)
            {
                for (int i = 0; i < 10; i++)
                {
                    var first = firstNames[(grade * 10 + i) % firstNames.Length];
                    var middle = middleNames[(grade * 7 + i) % middleNames.Length];

                    bool female = femaleNames.Contains(first);

                    var last = female
                        ? lastNamesFemale[(grade * 5 + i) % lastNamesFemale.Length]
                        : lastNamesMale[(grade * 5 + i) % lastNamesMale.Length];

                    string? section = null;
                    if (grade >= 8)
                        section = sections[i % 4];

                    clients.Add(new Client
                    {
                        FirstName = first,
                        MiddleName = middle,
                        LastName = last,
                        PhoneNumber = NextPhone(),
                        CardNumber = NextCard(),
                        Grade = grade,
                        Section = section,
                        IsActive = true,
                        CreatedOn = DateTime.Now.AddDays(-(grade * 10 + i))
                    });
                }

                for (int i = 0; i < 3; i++)
                {
                    var first = firstNames[(grade * 11 + i + 3) % firstNames.Length];
                    var middle = middleNames[(grade * 9 + i + 5) % middleNames.Length];

                    bool female = femaleNames.Contains(first);

                    var last = female
                        ? lastNamesFemale[(grade * 6 + i) % lastNamesFemale.Length]
                        : lastNamesMale[(grade * 6 + i) % lastNamesMale.Length];

                    string? section = null;
                    if (grade >= 8)
                        section = sections[i % 4];

                    clients.Add(new Client
                    {
                        FirstName = first,
                        MiddleName = middle,
                        LastName = last,
                        PhoneNumber = NextPhone(),
                        CardNumber = NextCard(),
                        Grade = grade,
                        Section = section,
                        IsActive = false,
                        CreatedOn = DateTime.Now.AddDays(-(300 + grade * 5 + i))
                    });
                }
            }

            await context.Clients.AddRangeAsync(clients);
            await context.SaveChangesAsync();
        }
    }
}