using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Service.Search;
using PBL3.ViewModels.Search;

namespace PBL3.Controllers
{
    public class UtilController : Controller
    {
        private readonly ISearchService _searchService;
        private const int PageSize = 20;
        private readonly ApplicationDbContext _dbContext;
        public UtilController(ISearchService searchService, ApplicationDbContext applicationDbContext)
        {
            _searchService = searchService;
            _dbContext = applicationDbContext;
        }

        // Tìm kiếm theo tên truyện hiện tại
        //GET: /Util/Search?tenTruyen={}&page={}
        public async Task<IActionResult> Search(string? tenTruyen, int page = 1)
        {
            List<SearchByTitleStory> results = await _searchService.SearchByTitleAsync(tenTruyen, page, PageSize);
            return View(results);
        }

        // Tìm kiếm nâng cao
        // GET: /Util/AdvancedSearch?page=1
        public async Task<IActionResult> AdvancedSearch([FromQuery] StoryFilterModel filter, int page = 1)
        {
            if (filter == null) filter = new StoryFilterModel();

            // Lấy danh sách thể loại từ DB
            var genres = await _dbContext.Genres
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem
                {
                    Value = g.Name,      
                    Text = g.Name
                })
                .ToListAsync();

            ViewBag.GenresList = genres;

            var results = await _searchService.SearchAdvancedAsync(filter, page, PageSize);

            ViewBag.Filter = filter;
            ViewBag.Page = page;

            ViewBag.StatusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "--Chọn--" },
                new SelectListItem { Value = "Active", Text = "Đang tiến hành" },
                new SelectListItem { Value = "Completed", Text = "Đã hoàn thành" }
            };

            return View(results);
        }

    }
}
