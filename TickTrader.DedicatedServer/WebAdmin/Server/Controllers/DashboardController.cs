using Microsoft.AspNetCore.Mvc;
using TickTrader.DedicatedServer.WebAdmin.Server.Dto;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    public class DashboardController: Controller
    {
        [HttpPost]
        public void Post([FromBody]PluginSetupDto setup)
        {
            var a = setup;
        }
    }
}
