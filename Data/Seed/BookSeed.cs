using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.Enums;

namespace ObreshkovLibrary.Data.Seed
{
    public static class BookSeed
    {
        public static async Task SeedBooksAsync(ObreshkovLibraryContext context)
        {
            var fiction = await context.Categories
                .FirstOrDefaultAsync(c => c.Name == "Художествена литература" && c.ParentCategoryId == null);

            var novelCategory = fiction == null
                ? null
                : await context.Categories.FirstOrDefaultAsync(c => c.Name == "Роман" && c.ParentCategoryId == fiction.Id);

            var storyCategory = fiction == null
                ? null
                : await context.Categories.FirstOrDefaultAsync(c => c.Name == "Разказ" && c.ParentCategoryId == fiction.Id);

            var childrenCategory = await context.Categories
                .FirstOrDefaultAsync(c => c.Name == "Детска литература" && c.ParentCategoryId == null);

            var fairyTaleCategory = childrenCategory == null
                ? null
                : await context.Categories.FirstOrDefaultAsync(c => c.Name == "Приказка" && c.ParentCategoryId == childrenCategory.Id);

            var scienceCategory = await context.Categories
                .FirstOrDefaultAsync(c => c.Name == "Научнопопулярна литература" && c.ParentCategoryId == null);

            var psychologyCategory = scienceCategory == null
                ? null
                : await context.Categories.FirstOrDefaultAsync(c => c.Name == "Психология" && c.ParentCategoryId == scienceCategory.Id);

            var schoolCategory = await context.Categories
                .FirstOrDefaultAsync(c => c.Name == "Учебна литература" && c.ParentCategoryId == null);

            var textbookCategory = schoolCategory == null
                ? null
                : await context.Categories.FirstOrDefaultAsync(c => c.Name == "Учебник" && c.ParentCategoryId == schoolCategory.Id);

            var workbookCategory = schoolCategory == null
                ? null
                : await context.Categories.FirstOrDefaultAsync(c => c.Name == "Помагало" && c.ParentCategoryId == schoolCategory.Id);

            var books = new List<BookSeedItem>
            {
                new BookSeedItem
                {
                    Title = "Ана Каренина",
                    Author = "Лев Толстой",
                    Description = "Романът изследва темите за любовта, брака, ревността и моралния избор в руското общество през XIX век.",
                    Year = 1878,
                    CopiesCount = 4,
                    SchoolClass = null,
                    CoverPath = "books/karenina.jpeg",
                    CategoryId = novelCategory?.Id ?? 0,
                    Tags =
                        BookTags.WorldLiterature |
                        BookTags.ClassicLiterature |
                        BookTags.Psychological |
                        BookTags.Social |
                        BookTags.Romantic |
                        BookTags.ForeignAuthor |
                        BookTags.ClassicalWork |
                        BookTags.Prose
                },

                new BookSeedItem
                {
                    Title = "Железният светилник",
                    Author = "Димитър Талев",
                    Description = "Романът проследява съдбата на възрожденското семейство Глаушеви и духовното пробуждане на българския народ.",
                    Year = 1952,
                    CopiesCount = 5,
                    SchoolClass = "11 клас",
                    CoverPath = "books/zhelezniat-svetilnik.jpg",
                    CategoryId = novelCategory?.Id ?? 0,
                    Tags =
                        BookTags.BulgarianLiterature |
                        BookTags.ClassicLiterature |
                        BookTags.Historical |
                        BookTags.Social |
                        BookTags.BulgarianAuthor |
                        BookTags.RequiredReading |
                        BookTags.ForMatura |
                        BookTags.ClassicalWork |
                        BookTags.Prose
                },

                new BookSeedItem
                {
                    Title = "Висящи дворци",
                    Author = "Ран Босилек",
                    Description = "Детско произведение, в което се съчетават въображение, доброта и поучителни послания.",
                    Year = 1941,
                    CopiesCount = 3,
                    SchoolClass = null,
                    CoverPath = "books/10396.250.jpg",
                    CategoryId = fairyTaleCategory?.Id ?? 0,
                    Tags =
                        BookTags.BulgarianLiterature |
                        BookTags.BulgarianAuthor |
                        BookTags.RecommendedReading |
                        BookTags.Prose
                },

                new BookSeedItem
                {
                    Title = "Речник на психоанализата",
                    Author = "Жан Лапланш и Жан-Бертран Понталис",
                    Description = "Второ издание на един от най-известните речници на психоанализата, появил се за първи път през 1967 г. и издаден впоследствие на 17 езика." +
                        "\r\n\r\nВ „Речник на психоанализата“ авторите си поставят задачата да анализират „концептуалния апарат на психоанализата, а именно - съвкупността от понятия, които тя постепенно изработва, за да отрази своите специфични открития“." +
                        " Намерението им е да осветлят първо началния смисъл на понятията и те съответно се насочват преди всичко към понятийния апарат на Зигмунд Фройд. Стремежът им е не толкова да инвентаризират изчерпателно езика на психоанализата, колкото да се задълбочат в смисъла на понятията и проблемите, свързани с тях.",
                    Year = 1967,
                    CopiesCount = 4,
                    SchoolClass = null,
                    CoverPath = "books/rechniknapsihoanalizata.jpg",
                    CategoryId = psychologyCategory?.Id ?? 0,
                    Tags =
                        BookTags.WorldLiterature |
                        BookTags.Psychological |
                        BookTags.EducationalContent |
                        BookTags.ForeignAuthor |
                        BookTags.Prose
                },

                new BookSeedItem
                {
                    Title = "Изкуството да обичаш",
                    Author = "Ерих Фром",
                    Description = "Човечеството не може да съществува нито ден без обич." +
                        "\r\n\r\n„Изкуството да обичаш“ на Ерих Фром е класическо произведение, което надниква в тайните на човешката душа. Прочутият психолог разглежда различните видове емоция: братската обич, майчината обич, еротичната обич, обичта към себе си, обичта към Бога и други.",
                    Year = 1956,
                    CopiesCount = 2,
                    SchoolClass = null,
                    CoverPath = "books/izkustvotodaobichash.jpg",
                    CategoryId = psychologyCategory?.Id ?? 0,
                    Tags =
                        BookTags.Psychological |
                        BookTags.EducationalContent |
                        BookTags.ForeignAuthor |
                        BookTags.ClassicalWork |
                        BookTags.Prose
                },

                new BookSeedItem
                {
                    Title = "Български език за 12. клас - задължителна подготовка",
                    Author = "Булвест 2000",
                    Description = "Учебникът по български език за 12. клас е предназначен за обучение в часовете за общообразователна подготовка по български език и литература." +
                        " Съобразен е с изискванията на новата учебна програма по български език за 12. клас по отношение на: вида, броя и формулировките на темите от учебното съдържание; последователността при представяне на темите; съотношението между уроците за нови знания, уроците за упражнение, уроците за преговор и обобщение, уроците за проверка и оценка на знанията и уменията.",
                    Year = 2024,
                    CopiesCount = 15,
                    SchoolClass = "12 клас",
                    CoverPath = "books/12BEL.jpg",
                    CategoryId = textbookCategory?.Id ?? 0,
                    Tags =
                        BookTags.EducationalContent |
                        BookTags.ForMatura |
                        BookTags.RequiredReading |
                        BookTags.BulgarianLanguage |
                        BookTags.Prose
                },

                new BookSeedItem
                {
                    Title = "Пробни изпити за матурата по български език и литература - 12. клас",
                    Author = "БГ Учебник",
                    Description = "Помагалото Пробни изпити за матурата по български език и литература съдържа седем цялостни теста върху целия изпитен материал по български език и литература от 11. клас и 12. клас." +
                        "\r\n\r\nВсеки тест е конструиран точно по изпитния формат на държавния зрелостен изпит. Съдържа три части с различен брой задачи. В първата част има 21 задачи, във втората - 19, а в третата - 1 задача за създаване на текст с две възможности - есе или интерпретативно съчинение. В края на помагалото са поместени листове за записване на отговорите на задачите от тестовете." +
                        "\r\n\r\nПомагалото е предназначено за финална проверка на знанията по български език и литература непосредствено преди изпита." +
                        "\r\n\r\nНастоящото издание е съвместимо с учебната програма за 2024/2025 г.",
                    Year = 2025,
                    CopiesCount = 10,
                    SchoolClass = "12 клас",
                    CoverPath = "books/pomagalo1.jpg",
                    CategoryId = workbookCategory?.Id ?? 0,
                    Tags =
                        BookTags.EducationalContent |
                        BookTags.RequiredReading |
                        BookTags.ForMatura |
                        BookTags.BulgarianLanguage
                },

                new BookSeedItem
                {
                    Title = "Подготовка за матура по български език и литература за 11. и 12. клас",
                    Author = "Колибри",
                    Description = "Учебното помагало е предназначено за подготовка на учениците за Държавния зрелостен изпит (ДЗИ) по български език и литература след завършен 12. клас." +
                        "\r\n\r\nПомагалото съдържа:" +
                        "\r\n\r\nтестове по тематични раздели за Държавен зрелостен изпит по български език и литература;" +
                        "\r\nобобщаващи тестове върху учебния материал от 11. клас, от 12. клас, от 11. и 12. клас;" +
                        "\r\nнасоки за междутекстови анализи върху изучавани и неизучавани творби." +
                        "\r\n\r\nМеждутекстовият анализ е все още нов подход, изучаван в часовете по литература. Третата част на помагалото съдържа насоки за цялостен модел за създаване на писмен интерпретативен междутекстов анализ. Предназначена е за работа както в задължителната, така и в профилираната подготовка по литература. Стремежът е да се посочат основни модели, по които може да се реализира междутекстов анализ, за да се овладее от учениците процесът на писането му.",
                    Year = 2023,
                    CopiesCount = 5,
                    SchoolClass = "12 клас",
                    CoverPath = "books/pomagalo3.jpg",
                    CategoryId = workbookCategory?.Id ?? 0,
                    Tags =
                        BookTags.EducationalContent |
                        BookTags.ForMatura |
                        BookTags.BulgarianLanguage
                },

                new BookSeedItem
                {
                    Title = "Успех на матурата по български език и литература - По учебната програма за 2024/2025 г.",
                    Author = "Клет България",
                    Description = "Помагалото Успех на матурата по български език и литература. Тестове за ДЗИ от поредицата Успех на матурата е предназначено за подготовка на ученици за задължителния държавен зрелостен изпит по български език и литература." +
                        "\r\n\r\nСлед внимателен анализ на примерните задачи и на вече използваните от Министерството на образованието и науката (МОН) изпитни формати ви предлагаме 10 теста по актуалния формат (в сила от учебната 2021/2022 година). Съдържанието, структурата и формулировките в изданието са в пълно съответствие с учебно-изпитната програма и с общите параметри за матурата по български език и литература, утвърдени с нормативни документи.",
                    Year = 2023,
                    CopiesCount = 3,
                    SchoolClass = "12 клас",
                    CoverPath = "books/pomagalo2.jpg",
                    CategoryId = workbookCategory?.Id ?? 0,
                    Tags =
                        BookTags.EducationalContent |
                        BookTags.RequiredReading |
                        BookTags.ForMatura |
                        BookTags.BulgarianLanguage
                },

                new BookSeedItem
{
    Title = "Разкази",
    Author = "Елин Пелин",
    Description = "Сборник с едни от най-известните разкази на Елин Пелин, свързани със селския живот, човешката съдба, бедността, труда и нравствените конфликти.",
    Year = 2004,
    CopiesCount = 5,
    SchoolClass = "5 клас, 6 клас, 7 клас, 10 клас",
    CoverPath = "books/elin-pelin-razkazi-geratsite.jpg",
    CategoryId = storyCategory?.Id ?? 0,
    Tags =
        BookTags.BulgarianLiterature |
        BookTags.ClassicLiterature |
        BookTags.Social |
        BookTags.BulgarianAuthor |
        BookTags.RequiredReading |
        BookTags.ForMatura |
        BookTags.ClassicalWork |
        BookTags.Prose,
    SearchKeywords = "Косачи; По жицата; На оня свят; Андрешко; Чорба от греховете на отец Никодим; Задушница; Напаст божия; Ветрената мелница; Мечтатели; разказ; разкази; сборник с разкази; Елин Пелин разкази"
}
            };

            foreach (var seed in books)
            {
                if (seed.CategoryId == 0)
                    continue;

                var existingBook = await context.Books
                    .Include(b => b.Copies)
                    .FirstOrDefaultAsync(b => b.Title == seed.Title && b.Author == seed.Author);

                if (existingBook != null)
                    continue;

                var book = new Book
                {
                    Title = seed.Title,
                    Author = seed.Author,
                    Description = seed.Description,
                    Year = seed.Year,
                    SchoolClass = seed.SchoolClass,
                    CoverPath = seed.CoverPath,
                    CategoryId = seed.CategoryId,
                    Tags = seed.Tags,
                    SearchKeywords = seed.SearchKeywords,
                    CreatedOn = DateTime.Now,
                    IsActive = true
                };

                context.Books.Add(book);
                await context.SaveChangesAsync();

                var copies = new List<BookCopy>();

                for (int i = 0; i < seed.CopiesCount; i++)
                {
                    copies.Add(new BookCopy
                    {
                        BookId = book.Id,
                        IsActive = true
                    });
                }

                context.BookCopies.AddRange(copies);
                await context.SaveChangesAsync();
            }

            await SeedArchivedBooksAsync(context,
                novelCategory?.Id ?? 0,
                storyCategory?.Id ?? 0,
                fairyTaleCategory?.Id ?? 0,
                psychologyCategory?.Id ?? 0,
                textbookCategory?.Id ?? 0,
                workbookCategory?.Id ?? 0);

            await SeedArchivedCopiesAsync(context);
        }

        private static async Task SeedArchivedBooksAsync(
            ObreshkovLibraryContext context,
            int novelCategoryId,
            int storyCategoryId,
            int fairyTaleCategoryId,
            int psychologyCategoryId,
            int textbookCategoryId,
            int workbookCategoryId)
        {
            var archivedBooks = new List<BookSeedItem>
            {
                new BookSeedItem
                {
                    Title = "Под игото - архив",
                    Author = "Иван Вазов",
                    Description = "Архивен тестов запис за роман от българската класика.",
                    Year = 1894,
                    CopiesCount = 2,
                    SchoolClass = "11 клас",
                    CategoryId = novelCategoryId,
                    Tags = BookTags.BulgarianLiterature | BookTags.ClassicLiterature | BookTags.BulgarianAuthor | BookTags.ClassicalWork | BookTags.Prose,
                    SearchKeywords = "архив; роман; Иван Вазов"
                },
                new BookSeedItem
                {
                    Title = "Старопланински легенди - архив",
                    Author = "Йордан Йовков",
                    Description = "Архивен тестов запис за сборник с разкази.",
                    Year = 1927,
                    CopiesCount = 2,
                    SchoolClass = "10 клас",
                    CategoryId = storyCategoryId,
                    Tags = BookTags.BulgarianLiterature | BookTags.ClassicLiterature | BookTags.BulgarianAuthor | BookTags.Prose,
                    SearchKeywords = "архив; разкази; Йовков"
                },
                new BookSeedItem
                {
                    Title = "Патиланско царство - архив",
                    Author = "Ран Босилек",
                    Description = "Архивен тестов запис за детска книга.",
                    Year = 1948,
                    CopiesCount = 1,
                    SchoolClass = null,
                    CategoryId = fairyTaleCategoryId,
                    Tags = BookTags.BulgarianLiterature | BookTags.BulgarianAuthor | BookTags.RecommendedReading | BookTags.Prose,
                    SearchKeywords = "архив; детска литература"
                },
                new BookSeedItem
                {
                    Title = "Основи на психологията - архив",
                    Author = "Мария Петрова",
                    Description = "Архивен тестов запис за научнопопулярно заглавие.",
                    Year = 2019,
                    CopiesCount = 1,
                    SchoolClass = null,
                    CategoryId = psychologyCategoryId,
                    Tags = BookTags.Psychological | BookTags.EducationalContent | BookTags.Prose,
                    SearchKeywords = "архив; психология"
                },
                new BookSeedItem
                {
                    Title = "История и цивилизации за 10. клас - архив",
                    Author = "Анубис",
                    Description = "Архивен тестов запис за учебник.",
                    Year = 2022,
                    CopiesCount = 2,
                    SchoolClass = "10 клас",
                    CategoryId = textbookCategoryId,
                    Tags = BookTags.EducationalContent | BookTags.History | BookTags.Prose,
                    SearchKeywords = "архив; учебник; история"
                },
                new BookSeedItem
                {
                    Title = "Литературни анализи за 12. клас - архив",
                    Author = "Просвета",
                    Description = "Архивен тестов запис за помагало по литература.",
                    Year = 2023,
                    CopiesCount = 2,
                    SchoolClass = "12 клас",
                    CategoryId = workbookCategoryId,
                    Tags = BookTags.EducationalContent | BookTags.ForMatura | BookTags.BulgarianLanguage | BookTags.Prose,
                    SearchKeywords = "архив; помагало; матура"
                }
            };

            foreach (var seed in archivedBooks)
            {
                if (seed.CategoryId == 0)
                    continue;

                var existingBook = await context.Books
                    .IgnoreQueryFilters()
                    .Include(b => b.Copies)
                    .FirstOrDefaultAsync(b => b.Title == seed.Title && b.Author == seed.Author);

                if (existingBook != null)
                    continue;

                var book = new Book
                {
                    Title = seed.Title,
                    Author = seed.Author,
                    Description = seed.Description,
                    Year = seed.Year,
                    SchoolClass = seed.SchoolClass,
                    CoverPath = seed.CoverPath,
                    CategoryId = seed.CategoryId,
                    Tags = seed.Tags,
                    SearchKeywords = seed.SearchKeywords,
                    CreatedOn = DateTime.Now.AddDays(-120),
                    IsActive = false
                };

                context.Books.Add(book);
                await context.SaveChangesAsync();

                var copies = new List<BookCopy>();
                for (int i = 0; i < seed.CopiesCount; i++)
                {
                    copies.Add(new BookCopy
                    {
                        BookId = book.Id,
                        IsActive = false
                    });
                }

                context.BookCopies.AddRange(copies);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedArchivedCopiesAsync(ObreshkovLibraryContext context)
        {
            var targetTitles = new[]
            {
                "Ана Каренина",
                "Железният светилник",
                "Разкази",
                "Изкуството да обичаш",
                "Български език за 12. клас - задължителна подготовка"
            };

            foreach (var title in targetTitles)
            {
                var book = await context.Books
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(b => b.Title == title && b.IsActive);

                if (book == null)
                    continue;

                bool hasInactiveCopy = await context.BookCopies
                    .IgnoreQueryFilters()
                    .AnyAsync(c => c.BookId == book.Id && !c.IsActive);

                if (hasInactiveCopy)
                    continue;

                context.BookCopies.Add(new BookCopy
                {
                    BookId = book.Id,
                    IsActive = false
                });
            }

            await context.SaveChangesAsync();
        }

        private class BookSeedItem
        {
            public string Title { get; set; } = null!;
            public string Author { get; set; } = null!;
            public string Description { get; set; } = null!;
            public int? Year { get; set; }
            public int CopiesCount { get; set; }
            public string? SchoolClass { get; set; }
            public string? CoverPath { get; set; }
            public int CategoryId { get; set; }
            public BookTags Tags { get; set; }
            public string? SearchKeywords { get; set; }
        }
    }
}