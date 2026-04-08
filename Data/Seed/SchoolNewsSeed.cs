using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Data.Seed
{
    public static class SchoolNewsSeed
    {
        public static async Task SeedSchoolNewsAsync(ObreshkovLibraryContext context)
        {
            var items = new List<SchoolNews>
            {
                new SchoolNews
                {
                    Title = "Архив: Ден на отворените врати 2025",
                    Summary = "Информация за проведен ден на отворените врати в ППМГ с демонстрации и срещи с бъдещи ученици.",
                    NewsUrl = "https://ppmg.example.com/archive/open-day-2025",
                    PublishedOn = DateTime.Today.AddMonths(-8),
                    CreatedOn = DateTime.Now.AddMonths(-8),
                    DisplayOrder = 101,
                    IsActive = false
                },
                new SchoolNews
                {
                    Title = "Архив: Коледен благотворителен базар",
                    Summary = "Публикация за благотворителния базар и събраните средства за училищна кауза.",
                    NewsUrl = "https://ppmg.example.com/archive/christmas-bazaar",
                    PublishedOn = DateTime.Today.AddMonths(-7),
                    CreatedOn = DateTime.Now.AddMonths(-7),
                    DisplayOrder = 102,
                    IsActive = false
                },
                new SchoolNews
                {
                    Title = "Архив: Областен кръг по математика",
                    Summary = "Новина за участието на ученици от ППМГ в областния кръг по математика.",
                    NewsUrl = "https://ppmg.example.com/archive/math-round",
                    PublishedOn = DateTime.Today.AddMonths(-6),
                    CreatedOn = DateTime.Now.AddMonths(-6),
                    DisplayOrder = 103,
                    IsActive = false
                },
                new SchoolNews
                {
                    Title = "Архив: Седмица на четенето",
                    Summary = "Материал за инициативите на библиотеката и литературните срещи по време на седмицата на четенето.",
                    NewsUrl = "https://ppmg.example.com/archive/reading-week",
                    PublishedOn = DateTime.Today.AddMonths(-5),
                    CreatedOn = DateTime.Now.AddMonths(-5),
                    DisplayOrder = 104,
                    IsActive = false
                },
                new SchoolNews
                {
                    Title = "Архив: Турнир по информатика",
                    Summary = "Кратка новина за училищния турнир по информатика и отличените участници.",
                    NewsUrl = "https://ppmg.example.com/archive/informatics-tournament",
                    PublishedOn = DateTime.Today.AddMonths(-4),
                    CreatedOn = DateTime.Now.AddMonths(-4),
                    DisplayOrder = 105,
                    IsActive = false
                },
                new SchoolNews
                {
                    Title = "Архив: Пролетен концерт на училището",
                    Summary = "Съобщение за проведен пролетен концерт с участие на ученици и гости.",
                    NewsUrl = "https://ppmg.example.com/archive/spring-concert",
                    PublishedOn = DateTime.Today.AddMonths(-3),
                    CreatedOn = DateTime.Now.AddMonths(-3),
                    DisplayOrder = 106,
                    IsActive = false
                }
            };

            foreach (var item in items)
            {
                var existing = await context.SchoolNews
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.Title == item.Title);

                if (existing != null)
                    continue;

                context.SchoolNews.Add(item);
            }

            await context.SaveChangesAsync();
        }
    }
}
