using Microsoft.AspNetCore.Mvc;

namespace CMCS.Controllers
{
    public class ClaimsController : Controller
    {
        //ALl the methods listed below all just return each of the corresponding view (temporary)
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult History()
        {
            return View();
        }
    }
}
