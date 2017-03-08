using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Logging;
using TickTrader.DedicatedServer.DS;
using TickTrader.DedicatedServer.WebAdmin.Server.Hubs;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : HubController<DSFeed>
    {
        private readonly ILogger<RepositoryController> _logger;
        private readonly IDedicatedServer _dedicatedServer;
        
        public AccountController(IConnectionManager connectionManager, IDedicatedServer ddServer, ILogger<RepositoryController> logger) :
            base(connectionManager)
        {
            _dedicatedServer = ddServer;
            _logger = logger;
        }

        [HttpGet]
        public void Get()
        {

        }

        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        [HttpPut]
        public void Put([FromBody]string value)
        {
        }

        [HttpDelete]
        public void Delete(string login, string server)
        {

        }
    }
}
