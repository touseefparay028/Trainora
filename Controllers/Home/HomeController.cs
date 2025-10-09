using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Controllers.Home
{
    public class HomeController : Controller
    {
   
        public IActionResult Index()
        {
            return View();
        }
        [Route("About")]
        public IActionResult About()
        {
            return View();
        }
        [Route("Join")]
        public IActionResult Join()
        {
            return View();
        }
        [Route("AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [Route("Features")]
        public IActionResult Features()
        {
            return View();
        }
    }
}
