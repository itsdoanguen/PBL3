using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.ViewModels.Story;
using PBL3.ViewModels.Chapter;
using PBL3.Service;
using System.Security.Claims;

namespace PBL3.Service
{
    public class StoryService : IStoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        private readonly IChapterService _chapterService;

        public StoryService(ApplicationDbContext context, IStoryService storyService, BlobService blobService, IChapterService chapterService)
        {
            _context = context;
            _blobService = blobService;
            _chapterService = chapterService;
        }

        public async Task<int?> CreateStoryAsync(StoryCreateViewModel model, int authorID)
        {
            var story = new StoryModel
            {
                Title = model.Title,
                Description = model.Description,
                AuthorID = authorID,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = StoryModel.StoryStatus.Inactive
            };

            await _context.Stories.AddAsync(story);
            await _context.SaveChangesAsync();

            if (model.GenreIDs != null && model.GenreIDs.Any())
            {
                var storyGenres = model.GenreIDs.Select(genreID => new StoryGenreModel
                {
                    StoryID = story.StoryID,
                    GenreID = genreID
                }).ToList();
                _context.StoryGenres.AddRange(storyGenres);
            }
            await _context.SaveChangesAsync();

            return story.StoryID;
        }

        public async Task DeleteStoryAsync(int StoryID)
        {
            // Xóa cmt của story
            var relatedCommentsStory = await _context.Comments
                .Where(c => c.StoryID == StoryID)
                .ToListAsync();
             _context.Comments.RemoveRange(relatedCommentsStory);


            // Xóa chapter từ IChapterSevice
            var chapters = await _context.Chapters
                .Where(c => c.StoryID == StoryID)
                .ToListAsync();
            foreach (var chapter in chapters)
            {
                await _chapterService.DeleteChapterAsync(chapter.ChapterID, StoryID);
            }

            // Xóa Story
            var story = await _context.Stories
                .Where(s => s.StoryID == StoryID)
                .FirstOrDefaultAsync();
            _context.Stories.Remove(story);
            await _context.SaveChangesAsync();
        }

        public async Task<StoryEditViewModel> getStoryEditDetailsAsync(int storyID, int authorID)
        {
            var story = await _context.Stories
                .Include(s => s.Chapters)
                .FirstOrDefaultAsync(s => s.StoryID == storyID && s.AuthorID == authorID);
            if (story == null)
            {
                return null;
            }
            var chapters = await _context.Chapters
                .Where(c => c.StoryID == storyID)
                .Select(c => new ChapterModel
                {
                    ChapterID = c.ChapterID,
                    Title = c.Title,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();
            return new StoryEditViewModel
            {
                StoryID = story.StoryID,
                Title = story.Title,
                Description = story.Description,
                //Chapters = chapters
            };
        }
    }
}
