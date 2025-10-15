using Microsoft.AspNetCore.Mvc;

namespace CMCS.Controllers
{
    public class LecturerController : Controller
    {
        //ALl the methods listed below all just return each of the corresponding view (temporary)
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Delete()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }
    }
}
