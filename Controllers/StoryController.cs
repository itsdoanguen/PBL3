using Microsoft.AspNetCore.Mvc;

namespace PBL3.Controllers
{
    public class StoryController : Controller
    {
        //GET: Story/Index
        public IActionResult Index()
        {
            return View();
        }
        //GET: Story/Create
        public IActionResult Create()
        {
            return View();
        }
        ////POST: Story/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create()
        //{
        //    return View();
        //}


        //GET: Story/Detail/{id}
        public IActionResult Detail(int id)
        {
            return View();
        }
    }
}
