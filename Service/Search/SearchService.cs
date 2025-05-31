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

        public async Task<List<SearchByTitleStory>> SearchByTitleAsync(string? tenTruyen, int pageNumber = 1, int pageSize = 20)
        {
            var query = _dbContext.Stories  .Where(s => s.Status == Models.StoryModel.StoryStatus.Active || s.Status == Models.StoryModel.StoryStatus.Completed)
                                            .AsQueryable();

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
                .Select(s => new SearchByTitleStory
                {
                    StoryID = s.StoryID,
                    Title = s.Title,
                    CoverImage = s.CoverImage,
                    AuthorName = s.Author != null ? s.Author.DisplayName ?? "Unknown" : "Unknown",
                    Status = s.Status.ToString(),
                    CreatedAt = s.CreatedAt,
                    Genres = s.Genres.Select(g => g.Genre.Name).ToList()
                })
                .ToListAsync();

            return results;
        }

        public async Task<List<SearchByTitleStory>> SearchAdvancedAsync(StoryFilterModel filter, int pageNumber = 1, int pageSize = 20)
        {
            var query = _dbContext.Stories  .Where(s => s.Status == Models.StoryModel.StoryStatus.Active || s.Status == Models.StoryModel.StoryStatus.Completed)
                                            .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.TenTruyen))
            {
                query = query.Where(s => EF.Functions.Like(s.Title, $"%{filter.TenTruyen}%"));
            }

            // Bỏ lọc theo GenreId, chỉ dùng lọc tên thể loại
            if (!string.IsNullOrWhiteSpace(filter.GenreName))
            {
                query = query.Where(s => s.Genres.Any(g => EF.Functions.Like(g.Genre.Name, $"%{filter.GenreName}%")));
            }

            if (filter.CreatedFrom.HasValue)
            {
                // Lấy từ đầu ngày
                var fromDate = filter.CreatedFrom.Value.Date;
                query = query.Where(s => s.CreatedAt >= fromDate);
            }

            if (filter.CreatedTo.HasValue)
            {
                // Lấy đến cuối ngày (23:59:59.999)
                var toDate = filter.CreatedTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(s => s.CreatedAt <= toDate);
            }


            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                if (Enum.TryParse<PBL3.Models.StoryModel.StoryStatus>(filter.Status, out var statusEnum))
                {
                    query = query.Where(s => s.Status == statusEnum);
                }
            }

            // Bỏ lọc theo AuthorId, chỉ dùng lọc theo tên tác giả
            if (!string.IsNullOrWhiteSpace(filter.AuthorName))
            {
                query = query.Where(s => EF.Functions.Like(s.Author.DisplayName, $"%{filter.AuthorName}%"));
            }

            var results = await query
                .Include(s => s.Author)
                .Include(s => s.Genres)
                    .ThenInclude(g => g.Genre)
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SearchByTitleStory
                {
                    StoryID = s.StoryID,
                    Title = s.Title,
                    CoverImage = s.CoverImage,
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
