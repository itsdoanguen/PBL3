using System.Threading.Tasks;

namespace PBL3.Service
{
    public interface IChapterService
    {
        Task DeleteChapterAsync(int chapterId, int storyId);
    }
}
