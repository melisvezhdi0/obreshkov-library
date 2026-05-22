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
                    Title = "Над 50 награди за ученици от ППМГ „Акад. Н. Обрешков“",
                    Summary = "Ученици от гимназията бяха отличени във Великденското математическо състезание и „Европейско кенгуру“.",
                    NewsUrl = "https://www.pmgrz.net/index.php/ychenitci/olimpiadi-i-sastezaniya/899-vreme-e-matematitzite-da-se-zabavlyavat",
                    ImagePath = "/uploads/school-news/math-awards.png",
                    PublishedOn = new DateTime(2026, 5, 20),
                    CreatedOn = new DateTime(2026, 5, 20),
                    DisplayOrder = 1,
                    IsActive = true
                },

                new SchoolNews
                {
                    Title = "Ученическият съвет посети Дома за стари хора",
                    Summary = "Ученици от ППМГ организираха посещение и театрална програма за възрастните хора в Разград.",
                    NewsUrl = "https://www.pmgrz.net/index.php/novini/897-nai-sardechnite-aplodismenti-za-teatralite-i-berna",
                    ImagePath = "/uploads/school-news/theater-visit.png",
                    PublishedOn = new DateTime(2026, 5, 18),
                    CreatedOn = new DateTime(2026, 5, 18),
                    DisplayOrder = 2,
                    IsActive = true
                },

                new SchoolNews
                {
                    Title = "Архив: Ден на отворените врати 2025",
                    Summary = "Информация за проведения ден на отворените врати с демонстрации и срещи с бъдещи ученици.",
                    NewsUrl = "https://ppmg.example.com/archive/open-day-2025",
                    ImagePath = "/uploads/school-news/archive-open-day.png",
                    PublishedOn = new DateTime(2025, 11, 12),
                    CreatedOn = new DateTime(2025, 11, 12),
                    DisplayOrder = 101,
                    IsActive = false
                },

                new SchoolNews
                {
                    Title = "Архив: Коледен благотворителен базар",
                    Summary = "Публикация за благотворителния базар и събраните средства за училищна кауза.",
                    NewsUrl = "https://ppmg.example.com/archive/christmas-bazaar",
                    ImagePath = "/uploads/school-news/archive-bazaar.png",
                    PublishedOn = new DateTime(2025, 12, 18),
                    CreatedOn = new DateTime(2025, 12, 18),
                    DisplayOrder = 102,
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