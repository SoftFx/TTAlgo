using Microsoft.AspNetCore.Mvc;

namespace TickTrader.BotAgent.WebAdmin.Server.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
