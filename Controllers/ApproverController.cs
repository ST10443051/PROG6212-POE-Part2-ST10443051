using Microsoft.AspNetCore.Mvc;

namespace CMCS.Controllers
{
    public class ApproverController : Controller
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

        public IActionResult Delete()
        {
            return View();
        }

        public IActionResult Functions()
        {
            return View();
        }

        public IActionResult PendingClaims()
        {
            return View();
        }
    }
}
