using Microsoft.AspNetCore.Mvc;
using PBL3.Service.Search;
using PBL3.ViewModels.Search;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PBL3.Controllers
{
    public class UtilController : Controller
    {
        private readonly ISearchService _searchService;

        public UtilController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        // URL: /Util/Search?tenTruyen=abc&page=1
        public async Task<IActionResult> Search(string? tenTruyen, int page = 1)
        {
            const int pageSize = 20;

            List<SearchViewModel> results = await _searchService.SearchByTitleAsync(tenTruyen, page, pageSize);

            return View(results); // View nhận model List<SearchViewModel>
        }
    }
}
