using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using System.Threading.Tasks;
using System.Linq;

namespace PBL3.Service
{
    public class ChapterService : IChapterService
    {
        private readonly ApplicationDbContext _context;
        public ChapterService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task DeleteChapterAsync(int chapterId, int storyId)
        {
            var chapter = await _context.Chapters.FindAsync(chapterId);
            if(chapter == null)
            {
                return;
            }

            var relatedComments = _context.Comments
                .Where(c => c.ChapterID == chapterId);
            _context.Comments.RemoveRange(relatedComments);

            var chapterToUpdate = await _context.Chapters
                .Where(c => c.ChapterOrder > chapter.ChapterOrder && c.StoryID == storyId)
                .ToListAsync();

            foreach(var c in chapterToUpdate)
            {
                c.ChapterOrder--;
            }
            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();
        }
    }
}
