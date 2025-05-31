using PBL3.Data;
using PBL3.ViewModels.Search;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PBL3.Service.Search
{
    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _dbContext;

        public SearchService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<SearchViewModel>> SearchByTitleAsync(string? tenTruyen, int pageNumber = 1, int pageSize = 20)
        {
            var query = _dbContext.Stories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tenTruyen))
            {
                query = query.Where(s => EF.Functions.Like(s.Title, $"%{tenTruyen}%"));
            }

            var results = await query
                .Include(s => s.Author)
                .Include(s => s.Genres)
                    .ThenInclude(g => g.Genre)
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SearchViewModel
                {
                    StoryID = s.StoryID,
                    Title = s.Title,
                    CoverImage = s.CoverImage,
                    Description = s.Description,
                    AuthorName = s.Author != null ? s.Author.DisplayName ?? "Unknown" : "Unknown",
                    Status = s.Status.ToString(),
                    CreatedAt = s.CreatedAt,
                    Genres = s.Genres.Select(g => g.Genre.Name).ToList()
                })
                .ToListAsync();

            return results;
        }
    }
}
