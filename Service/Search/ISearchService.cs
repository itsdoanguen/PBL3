using PBL3.ViewModels.Search;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PBL3.Service.Search
{
    public interface ISearchService
    {
        Task<List<SearchByTitleStory>> SearchByTitleAsync(string? tenTruyen, int pageNumber = 1, int pageSize = 20);
        Task<List<SearchByTitleStory>> SearchAdvancedAsync(StoryFilterModel filter, int pageNumber = 1, int pageSize = 20);
    }
}
